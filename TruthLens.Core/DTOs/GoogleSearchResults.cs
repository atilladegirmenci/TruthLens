using System.Text.Json.Serialization;

namespace TruthLens.Core.DTOs
{
    // --- Custom Search API Response Wrapper ---
    public class GoogleSearchResponse
    {
        [JsonPropertyName("items")]
        public List<GoogleSearchItem>? Items { get; set; }
    }

    public class GoogleSearchItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;
    }

    // --- Fact Check Tools API Response Wrapper ---
    public class FactCheckResponse
    {
        [JsonPropertyName("claims")]
        public List<Claim>? Claims { get; set; }
    }

    public class Claim
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("claimReview")]
        public List<ClaimReview>? ClaimReviews { get; set; }
    }

    public class ClaimReview
    {
        [JsonPropertyName("publisher")]
        public Publisher Publisher { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("textualRating")]
        public string TextualRating { get; set; } // e.g., "False", "True", "Misleading"

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class Publisher
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } // e.g., "Snopes", "Reuters"
    }
}