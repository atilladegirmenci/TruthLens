using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Shared.DTOs
{
    public class AnalysisResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public string AnalyzedContent { get; set; } = string.Empty;

        public float AiScore { get; set; }

        public string AiLabel { get; set; } = string.Empty;

        public string AiExplanation { get; set; } = string.Empty;

    }
}
