using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{
    public class TelemetryResult
    {
        public string entity { get; set; }
        public bool success { get; set; }
        public string data { get; set; }    
    }
}
