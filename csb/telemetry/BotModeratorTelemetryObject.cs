using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TL;

namespace csb.telemetry
{

    public class DailyPushes : ITelemetryResult
    {
        public int Sent { get; set; } = 0;
        public int Delivered { get; set; } = 0;

        public TelemetryResult Get()
        {
            TelemetryResult res = new TelemetryResult()
            {
                entity = "DailyPushes",
                success = /*(Sent > 0 && Delivered < Sent * 0.3)*/true,
                data = $"sent={Sent} delivered={Delivered}"
            };
            
            return res;
        }

        public void Reset()
        {
            Sent = 0;
            Delivered = 0;
        }
    }

    public class Subscribers : ITelemetryResult
    {        
        int approved;
        public int Approved {
            get => approved;
            set
            {
                approved = value;
            }
        }
        public int Unsubscribed { get; set; } = 0;
        public int Declined { get; set; } = 0;

        public TelemetryResult Get()
        {
            TelemetryResult res = new TelemetryResult()
            {
                entity = "Subscribers",
                success = true,
                data = $"approved={Approved} declined={Declined}"
            };
            Reset();
            return res;
        }

        public void Reset()
        {
            Approved = 0;
            Declined = 0;
            Unsubscribed = 0;
        }
    }

    public class PushStart : ITelemetryResult
    {
        public int Shown { get; set; }
        public int Clicked { get; set; }

        public TelemetryResult Get()
        {
            TelemetryResult res = new TelemetryResult()
            {
                entity = "PushStart",
                success = Clicked >= Shown * 0.3,
                data = $"shown={Shown} clicked={Clicked}"
            };
            return res;
        }

        public void Reset()
        {
            Shown = 0;
            Clicked = 0;
        }
    }

    public class BotModeratorTelemetryObject : BaseTelemetryObject
    {
        #region properties
        public DailyPushes DailyPushes { get; set; } = new();   
        public Subscribers Subscribers { get; set; } = new();
        public PushStart PushStart { get; set; } = new();
        #endregion        

        public override void Reset()
        {
            DailyPushes.Reset();
            Subscribers.Reset();
            PushStart.Reset();
        }

        public override List<string> GetErrors()
        {
            List<string> res = new();

            var dpr = DailyPushes.Get();
            var sbr = Subscribers.Get();   
            var psr = PushStart.Get();

            if (!dpr.success)
                res.Add($"{dpr.entity} {dpr.data}");
            if (!sbr.success)
                res.Add($"{sbr.entity} {sbr.data}");
            if (!psr.success)
                res.Add($"{psr.entity} {psr.data}");
            return res;
        }

    }
}
