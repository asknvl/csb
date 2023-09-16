using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{

    public class Startup : ITelemetryResult
    {
        public bool IsStartedOk { get; set; } = true;

        public TelemetryResult Get()
        {

            string msg = (IsStartedOk) ? "OK" : "FAIL";

            TelemetryResult res = new TelemetryResult() {
                entity = "Startup",
                success = IsStartedOk,
                data = msg
            };
            return res;
        }        
    }

    public class TGUserTelemetryObject : BaseTelemetryObject
    {
        #region properties
        public Startup Startup { get; set; } = new();
        #endregion

        public override List<string> GetErrors()
        {
            List<string> res = new();

            var stp = Startup.Get();

            if (!stp.success)
                res.Add($"{stp.entity} {stp.data}");

            return res;
        }

        public override void Reset()
        {
            var stp = Startup.Get();
        }
    }
}
