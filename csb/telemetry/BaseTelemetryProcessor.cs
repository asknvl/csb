using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{
    public abstract class BaseTelemetryProcessor
    {
        #region vars        
        System.Timers.Timer resetTimer;        
        #endregion

        #region properties
        protected string GeoTag { get; set; }
        public BaseTelemetryObject TelemetryObject { get; set; }
        protected Queue<String> Exceptions { get; set; } = new();
        #endregion

        public BaseTelemetryProcessor(string geotag)
        {
            GeoTag = geotag;

            DateTime now = DateTime.Now;
            DateTime midnight = new DateTime(now.Year, now.Month, now.Day).AddDays(1);
            double initialInterval = (midnight - now).TotalMilliseconds;

            resetTimer = new System.Timers.Timer(initialInterval);
            resetTimer.Interval = 24 * 60 * 60 * 1000;
            resetTimer.AutoReset = true;
            resetTimer.Elapsed += (s, e) => {
                Reset();
            };
            resetTimer.Start();
        }

        #region protected       
        #endregion


        #region public
        public void AddException(string mesage)
        {
            if (!Exceptions.Contains(mesage))
                Exceptions.Enqueue($"{mesage}");
        }

        public List<string> GetExceptions()
        {
            List<string> res = new();

            while (Exceptions.Count > 0)
            {
                res.Add(Exceptions.Dequeue());                
            }

            return res;
        }
        public virtual void Reset()
        {
            Exceptions.Clear();
            TelemetryObject.Reset();
        }
        #endregion        
    }
}
