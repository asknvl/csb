using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{    

    public class BotModeratorTelemetryProcessor : BaseTelemetryProcessor
    {
        public BotModeratorTelemetryProcessor(string geotag) : base(geotag)
        {
            TelemetryObject = new BotModeratorTelemetryObject();
        }

        #region public
        public override void Reset()
        {
            base.Reset();
            TelemetryObject.Reset();
        }
        #endregion
    }
}
