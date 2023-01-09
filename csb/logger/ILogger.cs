using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.logger
{
    public interface ILogger
    {
        bool EnableConsoleOutput { get; set; }
        bool EnableConsoleErrorOutput { get; set; } 

        void dbg(string text);
        void err(string text);
        void inf(string text);
    }
}
