using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruthLens.Core.Interfaces
{
    public interface IOcrService
    {
        string ExtractTextFromImage(byte[] imageBytes);
    }
}
