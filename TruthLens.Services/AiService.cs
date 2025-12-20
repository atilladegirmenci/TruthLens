using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruthLens.Core.DTOs;
using TruthLens.Core.Interfaces;
using TruthLens.Services;

namespace TruthLens.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ModelUrl = "https://router.huggingface.co/hf-inference/models/facebook/bart-large-mnli";

        public AiService(IConfiguration conf)
        {
            _httpClient = new HttpClient();
            _apiKey = conf["HuggingFaceSettings:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Hugging Face API key is not configured.");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<AiResultDto> AnalyzeTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) return new AiResultDto();

            string truncatedText = text.Length > 1000 ? text.Substring(0, 1000) : text;

            var candidateLabels = new[] {
                "Trusted News",      // Güvenilir
                "Satire",            // Mizah
             // "Conspiracy Theory", // Komplo Teorisi
                "Hoax",              // Uydurmaca
                "Propaganda"         // Propaganda
            };

            var payload = new
            {
                inputs = truncatedText,
                parameters = new
                {
                    candidate_labels = candidateLabels
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(ModelUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (responseString.Contains("loading"))
                    {
                        return new AiResultDto { Label = "LOADING", Score = 0, Explanation = "Model warming up..." };
                    }
                    return new AiResultDto { Label = "ERROR", Score = 0, Explanation = $"API Error: {response.StatusCode}" };
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseString, options);

                if (results != null && results.Count > 0)
                {
                    var bestPrediction = results.OrderByDescending(x => x.Score).First();

                    // if not Trusted News everything else is FAKE
                    bool isFake = bestPrediction.Label != "Trusted News";

                    var scoresDict = results
                        .OrderByDescending(r => r.Score)
                        .ToDictionary(r => r.Label, r => (double)r.Score * 100);

                    var explanationBuilder = new System.Text.StringBuilder();
                    explanationBuilder.AppendLine($"AI has analyzed the content based on linguistic patterns and writing style, this content is classified as '{bestPrediction.Label}'.");
                    
                    return new AiResultDto
                    {
                        Label = isFake ? "FAKE" : "REAL",
                        Score = bestPrediction.Score*100,
                        Explanation = explanationBuilder.ToString(),
                        CategoryScores = scoresDict
                        
                    };
                }

                return new AiResultDto { Label = "UNKNOWN", Score = 0, Explanation = "Unable to classify content." };
            }
            catch (Exception ex)
            {
                return new AiResultDto { Label = "ERROR", Score = 0, Explanation = $"Exception: {ex.Message}" };
            }
        }
    }
}