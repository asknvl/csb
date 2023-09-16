using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{
    public class TGUserTelemetryProcessor : BaseTelemetryProcessor
    {
        public TGUserTelemetryProcessor(string geotag) : base(geotag)
        {
            TelemetryObject = new TGUserTelemetryObject();
        }
    }
}
