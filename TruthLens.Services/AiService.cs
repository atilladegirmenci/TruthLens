using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruthLens.Core.DTOs;
using TruthLens.Core.Interfaces;

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

            // STRATEJİ DEĞİŞİKLİĞİ: Sadece "Fake/Real" değil, spesifik türleri soruyoruz.
            // Model gramere bakıp "Real" demesin diye "Conspiracy" ve "Hoax" gibi seçenekler ekledik.
            var candidateLabels = new[] {
                "Trusted News",      // Güvenilir
                "Conspiracy Theory", // Komplo Teorisi
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
                    // En yüksek puanlı etiketi bul
                    var bestPrediction = results.OrderByDescending(x => x.Score).First();

                    // "Trusted News" değilse, diğer her şey bizim için FAKE kategorisindedir.
                    bool isFake = bestPrediction.Label != "Trusted News";

                    // DETAYLI AÇIKLAMA: Tüm skorları kullanıcıya gösterelim
                    // Örn: [Conspiracy: %90, Trusted: %10]
                    var allScores = string.Join(", ", results.OrderByDescending(r => r.Score)
                                                             .Select(r => $"{r.Label} (%{r.Score * 100:F1})"));

                    return new AiResultDto
                    {
                        Label = isFake ? "FAKE" : "REAL",
                        Score = bestPrediction.Score * 100,
                        Explanation = isFake
                            ? $"AI DETECTION: This content is flagged as '{bestPrediction.Label}' with %{bestPrediction.Score * 100:F1} confidence. \nFull Analysis: [{allScores}]"
                            : $"AI DETECTION: This content appears to be 'Trusted News' with %{bestPrediction.Score * 100:F1} confidence. \nFull Analysis: [{allScores}]"
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