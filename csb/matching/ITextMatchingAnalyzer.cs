using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.matching
{
    internal interface ITextMatchingAnalyzer
    {
        void Add(string message);
        int Check(string message);
    }
}
