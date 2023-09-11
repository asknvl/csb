using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.telemetry
{
    public abstract class BaseTelemetryObject
    {
        public abstract void Reset();
        public abstract List<string> GetErrors();

    }
}
