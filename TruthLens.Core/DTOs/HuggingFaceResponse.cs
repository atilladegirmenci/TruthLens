using System.Text.Json.Serialization;

namespace TruthLens.Core.DTOs
{
    public class HuggingFaceResponse
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }
    }
}