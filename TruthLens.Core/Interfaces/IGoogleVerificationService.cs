using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.Interfaces
{
    public interface IGoogleVerificationService
    {
        Task<string> VerifyNewsAsync(string query);
    }
}
