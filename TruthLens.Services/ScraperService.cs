using System;
using System.Threading.Tasks;
using PuppeteerSharp;
using SmartReader;
using TruthLens.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace TruthLens.Services
{
    public class ScraperService : IScraperService
    {
        // HttpClient'ı static tutmak performans için daha iyidir
        private static readonly HttpClient _httpClient = new HttpClient();

        public ScraperService()
        {
            // Kendimizi en son sürüm Chrome gibi tanıtıyoruz (Kritik Nokta Burası!)
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            }
        }

        public async Task<string> ScrapeTextAsync(string url)
        {
            string rawText = "";

            if (url.Contains("x.com") || url.Contains("twitter.com"))
                rawText = await ScrapFromX(url);
            else
                rawText = await ScrapFromNewsSite(url);

            return CleanandFormatText(rawText);
        }

        private async Task<string> ScrapFromNewsSite(string url)
        {
            try
            {
                // ADIM 1: İçeriği SmartReader'a çektirmeden önce biz çekiyoruz.
                // Çünkü SmartReader'ın varsayılan isteği Azure'da engelleniyor.
                var htmlContent = await _httpClient.GetStringAsync(url);

                // ADIM 2: İndirdiğimiz HTML'i SmartReader'a veriyoruz.
                var reader = new Reader(url, htmlContent);
                var article = await reader.GetArticleAsync();

                if (article.IsReadable)
                {
                    return $"{article.Title}\n\n{article.TextContent}";
                }

                // Yedek Plan: Eğer SmartReader hala okuyamazsa ham HTML'den body'i almayı deneyebiliriz
                // Ama şimdilik hata mesajı dönelim.
                return "Site içeriği okunamadı (Bot koruması veya boş içerik).";
            }
            catch (Exception ex)
            {
                return $"Haber sitesi okuma hatası: {ex.Message}";
            }
        }

        private async Task<string> ScrapFromX(string url)
        {
            try
            {
                // AZURE UYARISI: Azure Web App'te Puppeteer çalıştırmak zordur.
                // Klasör izinleri yüzünden tarayıcı inemeyebilir.

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                // Azure için kritik ayarlar: --no-sandbox
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                };

                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();

                // X.com gibi siteler için User-Agent şarttır
                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                await page.GoToAsync(url);

                var selector = "div[data-testid='tweet']";

                // Timeout süresini biraz uzatalım (Azure yavaş olabilir)
                await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 10000 });

                var text = await page.EvaluateFunctionAsync<string>(
                    $"() => document.querySelector('{selector}').innerText"
                    );

                return text;
            }
            catch (Exception ex)
            {
                // Puppeteer Azure'da çalışmazsa buraya düşecektir.
                return $"X.com scraping hatası: {ex.Message}. (Azure ortamında Puppeteer yapılandırması eksik olabilir)";
            }
        }

        private string CleanandFormatText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string noHtml = Regex.Replace(input, "<.*?>", string.Empty);
            string decoded = System.Net.WebUtility.HtmlDecode(noHtml);
            string noDoubleSpaces = Regex.Replace(decoded, @"\s+", " ");

            return noDoubleSpaces.Trim();
        }
    }
}