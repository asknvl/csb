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

    public class Updates : ITelemetryResult
    {
        public uint UpdatesCntr { get; set; }
        public uint UpdatesCntrPrev { get; set; }   

        public TelemetryResult Get()
        {
            bool s = (UpdatesCntr != UpdatesCntrPrev);
            string m = $"UpdateCntr={UpdatesCntr} UpdateCntrPrev={UpdatesCntrPrev}";

            TelemetryResult res = new TelemetryResult()
            {
                entity = "Updates",
                success = s,
                data = m
            };
            return res;
        }
    }

    public class TGUserTelemetryObject : BaseTelemetryObject
    {
        #region properties
        public Startup Startup { get; set; } = new();
        public Updates Updates { get; set; } = new();
        #endregion

        public override List<string> GetErrors()
        {
            List<string> res = new();

            var stp = Startup.Get();
            var upd = Updates.Get();

            if (!stp.success)
                res.Add($"{stp.entity} {stp.data}");
            if (!upd.success)
                res.Add($"{upd.entity} {upd.data}");

            return res;
        }

        public override void Reset()
        {            
        }
    }
}
