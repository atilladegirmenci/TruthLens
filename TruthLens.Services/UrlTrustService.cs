using System;
using System.Text.RegularExpressions; // Regex için gerekli
using System.Threading.Tasks;
using TruthLens.Core.DTOs;
using TruthLens.Core.Interfaces;
using Whois;

namespace TruthLens.Services
{
    public class UrlTrustService : IUrlTrustService
    {
        public async Task<UrlTrustResponse> AnalyzeDomainTrustAsync(string url)
        {
            var result = new UrlTrustResponse();

            try
            {
                // 1. URL Temizliği: "https://" ve "www." kısımlarını atıyoruz.
                // Whois sorguları genelde "google.com" şeklinde saf domain ister.
                var uri = new Uri(url);
                var host = uri.Host.Replace("www.", "");

                var whois = new WhoisLookup();
                var response = await whois.LookupAsync(host);

                // Tarihi yakalamak için değişkenimiz
                DateTime? creationDate = response.Registered;

                // --- B PLANI: Regex ile Elle Arama ---
                // Eğer kütüphane tarihi bulamadıysa (null geldiyse), ham metne (Content) bakarız.
                if (!creationDate.HasValue && !string.IsNullOrEmpty(response.Content))
                {
                    creationDate = ParseDateFromRawOutput(response.Content);
                }

                // --- ANALİZ ---
                if (creationDate.HasValue)
                {
                    var dateValue = creationDate.Value;
                    var age = DateTime.Now - dateValue;
                    int daysOld = (int)age.TotalDays;
                    int yearsOld = daysOld / 365;

                    result.DomainAgeDays = daysOld;

                    if (daysOld < 30)
                    {
                        result.IsTrusted = false;
                        result.TrustLabel = "⛔ HIGH RISK";
                        result.Details = $"WARNING! This site was registered just {daysOld} days ago. High probability of being a fake news or scam site.";
                    }
                    else if (daysOld < 365)
                    {
                        result.IsTrusted = false;
                        result.TrustLabel = "⚠️ MEDIUM RISK";
                        result.Details = $"This site is {daysOld} days old (less than a year). It may not have an established reputation yet.";
                    }
                    else
                    {
                        result.IsTrusted = true;
                        result.TrustLabel = "✅ TECHNICALLY TRUSTED";
                        result.Details = $"Domain has been active for {yearsOld} years. It has a long-standing history.";
                    }
                }
                else
                {
                    // Hem kütüphane hem de Regex bulamadıysa
                    result.IsTrusted = false;
                    result.DomainAgeDays = 0;
                    result.TrustLabel = "❔ UNKNOWN";
                    result.Details = "Domain registration date is hidden/redacted or could not be parsed. Proceed with caution.";
                }
            }
            catch (Exception ex)
            {
                result.IsTrusted = false;
                result.TrustLabel = "❌ ERROR";
                result.Details = $"Reliability analysis failed: {ex.Message}";
            }

            return result;
        }

        // --- YARDIMCI METOD: Regex ile Tarih Bulma ---
        private DateTime? ParseDateFromRawOutput(string content)
        {
            try
            {
                // Whois çıktılarında tarih genelde bu etiketlerle başlar:
                string[] patterns = new[]
                {
                    @"Creation Date:\s*(.*)",
                    @"Created on:\s*(.*)",
                    @"Registered on:\s*(.*)",
                    @"Domain Name Commencement Date:\s*(.*)",
                    @"created:\s*(.*)"
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string dateString = match.Groups[1].Value.Trim();
                        // Tarihi parse etmeyi dene
                        if (DateTime.TryParse(dateString, out DateTime parsedDate))
                        {
                            return parsedDate;
                        }
                    }
                }
            }
            catch
            {
                // Parsing hatası olursa null dön
            }
            return null;
        }
    }
}