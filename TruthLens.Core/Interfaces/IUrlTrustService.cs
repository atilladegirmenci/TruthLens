using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TruthLens.Core.DTOs;

namespace TruthLens.Core.Interfaces
{
    public interface IUrlTrustService
    {
        Task<UrlTrustResponse> AnalyzeDomainTrustAsync(string url);
    }
}
