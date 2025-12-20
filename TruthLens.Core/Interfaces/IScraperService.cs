using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.Interfaces
{
    public interface IScraperService
    {
        Task<string> ScrapeTextAsync(string url);
    }
}
