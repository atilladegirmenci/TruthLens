using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var uri = new Uri(url);
                var host = uri.Host;

                var whois = new WhoisLookup();
                var response = await whois.LookupAsync(host);

                if (response.Registered.HasValue)
                {
                    var creationDate = response.Registered.Value;
                    var age = DateTime.Now - creationDate;
                    int daysOld = (int)age.TotalDays;
                    int yearsOld = daysOld / 365;

                    result.DomainAgeDays = daysOld;

                    // --- LOGIC ---
                    // < 30 days: HIGH RISK (High probability of Phishing/Fake)
                    // < 1 year: SUSPICIOUS (Not established yet)
                    // > 1 year: TRUSTED (Technically)

                    if (daysOld < 30)
                    {
                        result.IsTrusted = false;
                        result.TrustLabel = "⛔ HIGH RISK";
                        result.Details = $"WARNING! This site was registered just {daysOld} days ago. High probability of being a fake news or scam site.";
                    }
                    else if (daysOld < 365)
                    {
                        result.IsTrusted = false; // Approaching with caution
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
                    // Whois date hidden or not found
                    result.IsTrusted = false;
                    result.DomainAgeDays = 0;
                    result.TrustLabel = "❔ UNKNOWN";
                    result.Details = "Domain registration date is hidden or could not be determined. Proceed with caution.";
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
    }
}