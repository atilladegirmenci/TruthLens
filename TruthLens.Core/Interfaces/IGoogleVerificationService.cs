using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruthLens.Core.DTOs;

namespace TruthLens.Core.Interfaces
{
    public interface IGoogleVerificationService
    {
        Task<(FactCheckResponse? FactCheck,GoogleSearchResponse? WebSearch)> VerifyNewsAsync(string query);
    }
}
