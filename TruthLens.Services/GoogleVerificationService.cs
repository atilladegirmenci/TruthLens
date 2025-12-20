using System.Net.Http.Json;
using TruthLens.Core.DTOs;
using TruthLens.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TruthLens.Services
{
    public class GoogleVerificationService : IGoogleVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _searchEngineId;

        public GoogleVerificationService(IConfiguration conf, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = conf["GoogleSettings:ApiKey"];
            _searchEngineId = conf["GoogleSettings:SearchEngineId"];
        }

        public async Task<(FactCheckResponse? FactCheck, GoogleSearchResponse? WebSearch)> VerifyNewsAsync(string query)
        {
            FactCheckResponse? factCheckData = null;
            GoogleSearchResponse? searchData = null;

            if (string.IsNullOrEmpty(query)) return (null, null);

            // Query optimization
            string optimizedQuery = query.Length > 200 ? query.Substring(0, 200) : query;
            string encodedQuery = Uri.EscapeDataString(optimizedQuery);

            // 1. FACT CHECK API
            try
            {
                var factUrl = $"https://factchecktools.googleapis.com/v1alpha1/claims:search?query={encodedQuery}&key={_apiKey}&languageCode=en";
                factCheckData = await _httpClient.GetFromJsonAsync<FactCheckResponse>(factUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FactCheck API Error: {ex.Message}");
            }

            // 2. CUSTOM SEARCH API
            try
            {
                var searchUrl = $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_searchEngineId}&q={encodedQuery}";
                searchData = await _httpClient.GetFromJsonAsync<GoogleSearchResponse>(searchUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomSearch API Error: {ex.Message}");
            }

            return (factCheckData, searchData);
        }
    }
}