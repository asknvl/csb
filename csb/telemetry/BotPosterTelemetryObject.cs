using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{



    public class BotPosterTelemetryObject : BaseTelemetryObject
    {
        public override List<string> GetErrors()
        {
            List<string> res = new List<string>();
            return res;
        }

        public override void Reset()
        {            
        }
    }
}
