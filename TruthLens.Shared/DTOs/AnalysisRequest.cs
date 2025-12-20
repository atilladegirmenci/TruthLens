using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Shared.DTOs
{
    public class AnalysisRequest
    {
        // Text or Image or url
        public string InputType { get; set; } = "Text";

        public string Content { get; set; } = string.Empty;
    }
}
