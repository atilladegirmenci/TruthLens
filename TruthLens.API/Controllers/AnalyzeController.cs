using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TruthLens.Core.DTOs;
using TruthLens.Core.Interfaces;
using TruthLens.Shared.DTOs;


namespace TruthLens.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IOcrService _ocrService;
        private readonly IScraperService _scraperService;
        private readonly IAiService _aiService;
        private readonly IGoogleVerificationService _googleVerificationService;
        private readonly ILLMService _llmService;
        public AnalyzeController(IOcrService ocrService, IScraperService scraperService, IAiService aiService, IGoogleVerificationService googleVerificationService, ILLMService llmService)
        {
            _googleVerificationService = googleVerificationService;
            _ocrService = ocrService;
            _scraperService = scraperService;
            _aiService = aiService;
            _llmService = llmService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult>  Analyze([FromBody] AnalysisRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
                return BadRequest("content is empty");

            var response = new AnalysisResponse();

            try
            {
                if (request.InputType.ToLower() == "image")
                {
                    byte[] imageBytes = Convert.FromBase64String(request.Content);

                    string extractedText = _ocrService.ExtractTextFromImage(imageBytes);

                    response.AnalyzedContent = extractedText;
                    response.Message = "Image analyzed successfully and text has submitted";
                }
                else if (request.InputType.ToLower() == "text")
                {
                    response.AnalyzedContent = request.Content;
                    response.Message = "Text has submitted successfully";
                }
                else if (request.InputType.ToLower() == "url")
                {
                    string scrapedText = await _scraperService.ScrapeTextAsync(request.Content);

                    response.AnalyzedContent = scrapedText;
                    response.Message = "URL content analyzed successfully and text has submitted";
                }

                if(string.IsNullOrEmpty(response.AnalyzedContent)) return BadRequest("No content to analyze.");
                
                var aiResult = await _aiService.AnalyzeTextAsync(response.AnalyzedContent);
                response.AiScore = aiResult.Score;
                response.AiLabel = aiResult.Label;
                response.CategoryScores = aiResult.CategoryScores;
                response.AiExplanation = aiResult.Explanation;

                var llmResult = await _llmService.AnalyzeTextAsync(response.AnalyzedContent);
                response.LlmComment = llmResult.Comment;

                var googleResults = await _googleVerificationService.VerifyNewsAsync(response.AnalyzedContent);

                if (googleResults.FactCheck?.Claims != null && googleResults.FactCheck.Claims.Count > 0)
                {
                    var bestClaim = googleResults.FactCheck.Claims.First();
                    var review = bestClaim.ClaimReviews?.FirstOrDefault();

                    if (review != null)
                    {
                        response.FactCheckResult = $"Verdict: \"{review.TextualRating.ToUpperInvariant()}\" by {review.Publisher.Name}.";
                    }

                    // Fact Check links in the Similar News section
                    foreach (var claim in googleResults.FactCheck.Claims)
                    {
                        if (claim.ClaimReviews == null) continue;
                        foreach (var r in claim.ClaimReviews)
                        {
                            response.SimilarNews.Add(new RelatedNewsItem
                            {
                                Title = string.IsNullOrEmpty(r.Title) ? claim.Text : r.Title,
                                Url = r.Url,
                                Source = r.Publisher?.Name ?? "Fact Check"
                            });
                        }
                    }
                }
                if (googleResults.WebSearch?.Items != null)
                {
                    foreach (var item in googleResults.WebSearch.Items)
                    {
                        // if same link exist doesnt add again
                        if (!response.SimilarNews.Any(x => x.Url == item.Link))
                        {
                            response.SimilarNews.Add(new RelatedNewsItem
                            {
                                Title = item.Title,
                                Url = item.Link,
                                Source = new Uri(item.Link).Host // Domain name
                            });
                        }
                    }
                }

                response.IsSuccess = true;
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new AnalysisResponse
                {
                    IsSuccess = false,
                    Message = $"An error occurred during analysis: {ex.Message}"
                });
                    
            }

        }
    }
}
