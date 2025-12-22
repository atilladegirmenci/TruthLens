using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.DTOs
{
    public class LLMResult
    {
        public string Comment { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
