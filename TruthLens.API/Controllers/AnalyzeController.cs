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

        public AnalyzeController(IOcrService ocrService, IScraperService scraperService, IAiService aiService, IGoogleVerificationService googleVerificationService)
        {
            _googleVerificationService = googleVerificationService;
            _ocrService = ocrService;
            _scraperService = scraperService;
            _aiService = aiService;
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
                    response.Message = "Image analyzed successfully";
                }
                else if (request.InputType.ToLower() == "text")
                {
                    response.AnalyzedContent = request.Content;
                    response.Message = "Text analyzed successfully";
                }
                else if (request.InputType.ToLower() == "url")
                {
                    string scrapedText = await _scraperService.ScrapeTextAsync(request.Content);

                    response.AnalyzedContent = scrapedText;
                    response.Message = "URL content analyzed successfully";
                }

                if(!string.IsNullOrEmpty(response.AnalyzedContent))
                {
                    var aiResult = await _aiService.AnalyzeTextAsync(response.AnalyzedContent);

                    response.AiScore = aiResult.Score;
                    response.AiLabel = aiResult.Label;
                    var verificationResult = await _googleVerificationService.VerifyNewsAsync(response.AnalyzedContent);
                    response.AiExplanation = $"{verificationResult} \n\n{aiResult.Explanation}";

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
