using System;
using System.Net.Http;
using System.Threading.Tasks;
using TruthLens.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using SmartReader; // SmartReader kütüphanesi tekrar sahnede!

namespace TruthLens.Services
{
    public class ScraperService : IScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly string _scraperApiKey;

        public ScraperService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Azure'a "ScraperApiSettings__ApiKey" olarak ekleyeceğiz
            _scraperApiKey = configuration["ScraperApiSettings:ApiKey"];
        }

        public async Task<string> ScrapeTextAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return "URL boş olamaz.";

            try
            {
                // ScraperAPI'yi kullanarak siteye gidiyoruz.
                // render=true : JavaScript çalıştıran siteler (Twitter/X gibi) için gereklidir.
                var targetUrl = $"http://api.scraperapi.com?api_key={_scraperApiKey}&url={Uri.EscapeDataString(url)}&render=true";

                // 1. ScraperAPI üzerinden sitenin HTML kaynağını çek
                var response = await _httpClient.GetAsync(targetUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Siteye erişilemedi. Hata Kodu: {response.StatusCode} (ScraperAPI)";
                }

                var htmlContent = await response.Content.ReadAsStringAsync();

                // 2. İndirdiğimiz HTML'i SmartReader ile analiz et (Sadece metni al)
                var reader = new Reader(url, htmlContent);
                var article = await reader.GetArticleAsync();

                if (article.IsReadable)
                {
                    // Başlık ve İçeriği birleştirip dönüyoruz
                    return $"BAŞLIK: {article.Title}\n\nİÇERİK:\n{article.TextContent}";
                }
                else
                {
                    // Eğer SmartReader okuyamazsa ham HTML'den body text'i almaya çalışalım (Yedek plan)
                    // Ama şimdilik basit bir mesaj dönelim.
                    return "Site içeriği metne dönüştürülemedi (İçerik çok kısa veya karmaşık).";
                }
            }
            catch (Exception ex)
            {
                return $"Scraping hatası: {ex.Message}";
            }
        }
    }
}