using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruthLens.Core.Interfaces;
using Tesseract;
using System.Text.RegularExpressions;

namespace TruthLens.Services
{
    public class OcrService : IOcrService
    {
        private readonly string _tessDataPath;

        // this will be changed later to support multiple languages
        public OcrService()
        {
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        }

        public string ExtractTextFromImage(byte[] imageBytes)
        {
            try
            {
                using (var engine = new TesseractEngine(_tessDataPath, "tur" ,EngineMode.Default))
                {

                    using (var img = Pix.LoadFromMemory(imageBytes))
                    {
                        using (var page = engine.Process(img))
                        {
                            var confidence = page.GetMeanConfidence();
                            string text = page.GetText();

                            if (string.IsNullOrEmpty(text))
                                return "can not read the text from image";

                            return CleanOcrText(text);
                            
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return $"OCR processing failed: {e.Message} , path: {_tessDataPath}";
            }
        }

        private string CleanOcrText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Remove unwanted characters and excessive whitespace
            string cleanedText = text.Replace("\n", " ").Replace("\r", " ");
            cleanedText = Regex.Replace(cleanedText, @"\s+", " "); // Replace multiple spaces with a single space

            return cleanedText.Trim();
        }
    }
}