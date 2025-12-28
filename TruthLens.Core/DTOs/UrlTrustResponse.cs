using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.DTOs
{
    public class UrlTrustResponse
    {
        public bool IsTrusted { get; set; }     // Teknik olarak güvenilir mi?
        public int DomainAgeDays { get; set; }  // Kaç günlük site?
        public string TrustLabel { get; set; }  // "Çok Riskli", "Güvenilir" vs.
        public string Details { get; set; }     // "Bu site 15 yıl önce kurulmuş..."
    }
}
