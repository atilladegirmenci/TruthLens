using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruthLens.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using TruthLens.Core.DTOs;
using System.Text.Json;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;


namespace TruthLens.Services
{
    public class LLMService : ILLMService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiModel = "gemini-3-flash-preview";

        public LLMService(IConfiguration conf)
        {
            _httpClient = new HttpClient();
            _apiKey = conf["GeminiSettings:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Gemini API key is not configured.");
            }
        }
        public async Task< LLMResult> AnalyzeTextAsync(string text)
        {
            var result =new LLMResult();

            var prompt = $@"
                 You are an expert fact-checker and news analyst. 
                 Analyze the following text primarily for its factual accuracy and logical consistency based on your general knowledge.
                 You can check the web for the latest information.
    
                 Evaluate whether the claims made are plausible, if the reasoning is sound, or if the information contradicts established facts. 
                 You may also briefly note if the tone or structure undermines its credibility, but keep the focus on the information itself.

                  Provide your assessment in a single, concise paragraph consisting of around 3 sentences.

                  Text: {text} ";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";

            var requestPayload = new
            {
                contents = new[]
                  {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };
            //json to string content
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");


            try
            {
                var response = await _httpClient.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API Error ({response.StatusCode}): {errorJson}");
                }

                // DESERIALIZATION
                var responseString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponseModel>(responseString, options);

                var analysisText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                result.Comment = analysisText ?? "Analiz yapılamadı veya boş sonuç döndü.";
                result.IsSuccess = true; 
            }
            catch (Exception ex)
            {
                result.Comment = $"Bir hata oluştu: {ex.Message}";
                result.IsSuccess = false;
            }

            return result;
        }

    }
    public class GeminiResponseModel
    {
        public GeminiCandidate[] Candidates { get; set; }
    }
    public class GeminiCandidate
    {
        public GeminiContent Content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[] Parts { get; set; }
    }

    public class GeminiPart
    {
        public string Text { get; set; }
    }
}
