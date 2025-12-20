using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;
using SmartReader;
using TruthLens.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Web;

namespace TruthLens.Services
{
    public class ScraperService : IScraperService
    {
        
        public async Task<string> ScrapeTextAsync(string url)
        {
            string rawText = "";

            if (url.Contains("x.com")) 
                rawText = await ScrapFromX(url);
            else 
                rawText = await ScrapFromNewsSite(url);

            return CleanandFormatText(rawText);
        }

        private async Task<string> ScrapFromNewsSite(string url)
        {
            try
            {
                var reader = new Reader(url);
                var article = await reader.GetArticleAsync();

                if(article.IsReadable)
                {
                    return $"{article.Title}\n\n{article.TextContent}";
                }

                return "site content is not readable";

            }
            catch (Exception ex)
            {
                return $"Error scraping news site: {ex.Message}";
            }
        }

        private async Task<string> ScrapFromX(string url)
        {
            try
            {
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                await page.GoToAsync(url);

                var selector = "div[data-testid='tweet']";
                await page.WaitForSelectorAsync(selector);

                var text = await page.EvaluateFunctionAsync<string>(
                    $"() => document.querySelector('{selector}').innerText"
                    );

                return text;
            }
            catch (Exception ex)
            {
                return $"Error scraping X.com: {ex.Message}";
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
