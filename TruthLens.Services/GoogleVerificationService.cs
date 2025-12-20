using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruthLens.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TruthLens.Core.DTOs;
using System.Globalization;

namespace TruthLens.Services
{
    public class GoogleVerificationService : IGoogleVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _searchEngineId;

        public GoogleVerificationService(IConfiguration conf)
        {
            _httpClient = new HttpClient();
            _apiKey = conf["GoogleSettings:ApiKey"];
            _searchEngineId = conf["GoogleSettings:SearchEngineId"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_searchEngineId))
            {
                throw new Exception("Google API key or Search Engine ID is not configured.");
            }
        }

        public async Task<string> VerifyNewsAsync(string query)
        {
            if (string.IsNullOrEmpty(query)) return "No query provided for verification";

            string optimizedQuery = query.Length > 200 ? query.Substring(0, 200) : query;

            var factCheckedResult = await CheckFactCheckApi(optimizedQuery);
            var customSearchResult = await CheckCustomSearchApi(optimizedQuery);

            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(factCheckedResult))
            {
                sb.AppendLine(factCheckedResult);
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(customSearchResult))
            {
                sb.Append(customSearchResult);
            }

            return sb.ToString();
        }

        private async Task<string> CheckFactCheckApi(string query)
        {
            try
            {
                var url = $"https://factchecktools.googleapis.com/v1alpha1/claims:search?query={Uri.EscapeDataString(query)}&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return string.Empty;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<FactCheckResponse>(json);

                if (data?.Claims != null && data.Claims.Count > 0)
                {
                    var review = data.Claims[0].ClaimReviews?.FirstOrDefault();

                    if (review != null)
                    {
                        return $"[FACT-CHECK]: This claim has been investigated by '{review.Publisher.Name}'. Verdict: {review.TextualRating}. (Source: {review.Title})";
                    }
                }
                return string.Empty;

            }
            catch (Exception ex)
            {
                // Log exception (not implemented here)
                return string.Empty;
            }
        }

        private async Task<string> CheckCustomSearchApi(string query)
        {
            try
            {
                var url = $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_searchEngineId}&q={Uri.EscapeDataString(query)}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return "could not veriy with google.";

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<GoogleSearchResponse>(json);

                if (data?.Items != null && data.Items.Count > 0)
                {
                    var sources = data.Items.Take(3).Select(i => i.Title).ToList();
                    string sourcesStr = string.Join(", ", sources);
                    return $"[WEB SEARCH]: Similar content found in these sources: {sourcesStr}.";
                }

                return "[WARNING]: No matching news found in trusted sources. This might be fake or breaking news.";
            }

            catch (Exception e)
            {
                return $"[ERROR]: Exception during Google Search API call: {e.Message}";
            }
        }
    }


}
