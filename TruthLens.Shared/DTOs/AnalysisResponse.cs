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

        public Dictionary<string, double> CategoryScores { get; set; } = new();

        public string? FactCheckResult { get; set; }

        public List<RelatedNewsItem> SimilarNews { get; set; } = new();

    }

    public class RelatedNewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
