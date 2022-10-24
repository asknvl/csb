using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.logger
{
    public interface ILogger
    {
        void dbg(string message);
        void err(string message);
    }
}
