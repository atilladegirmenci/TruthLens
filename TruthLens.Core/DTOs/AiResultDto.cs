using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.DTOs
{
    public class AiResultDto
    {
        public string Label { get; set; }
        public float Score { get; set; }
        public string Explanation { get; set; }
    }
}
