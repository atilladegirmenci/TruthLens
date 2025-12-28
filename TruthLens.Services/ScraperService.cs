using System;
using System.Net.Http;
using System.Threading.Tasks;
using TruthLens.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace TruthLens.Services
{
    public class ScraperService : IScraperService
    {
        private readonly HttpClient _httpClient;
        // Jina AI ücretsizdir ama yoğun kullanımda key isteyebilir. 
        // Şimdilik keysiz çalışır, ilerde gerekirse header'a eklersin.

        public ScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Jina AI bazen isteğin robottan geldiğini anlayınca JSON dönebiliyor.
            // Biz direkt metin istediğimiz için Header ayarı yapalım.
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "TruthLensApp/1.0");
            }
            if (!_httpClient.DefaultRequestHeaders.Contains("x-respond-with"))
            {
                // Bize direkt metin (Markdown) dönmesini söylüyoruz
                _httpClient.DefaultRequestHeaders.Add("x-respond-with", "text");
            }
        }

        public async Task<string> ScrapeTextAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return "URL boş olamaz.";

            try
            {
                // --- JINA AI---
                var targetUrl = $"https://r.jina.ai/{url}";

                var response = await _httpClient.GetAsync(targetUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(content))
                        return "İçerik boş döndü.";

                    // Jina AI genelde "Title" bilgisini metnin en başına koyar.
                    // Markdown formatında geldiği için temizlemeye bile gerek yok, 
                    // ama senin temizleme fonksiyonunu yine de kullanabiliriz.

                    // X.com (Twitter) için özel not:
                    // Jina AI Twitter'ı da okuyabilir ama bazen giriş ekranına takılabilir.
                    // Yine de Puppeteer'dan çok daha stabil çalışır.

                    return content;
                }
                else
                {
                    return $"Site okunamadı. Hata Kodu: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Scraping hatası: {ex.Message}";
            }
        }
    }
}