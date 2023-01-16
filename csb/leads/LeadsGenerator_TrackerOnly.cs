using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.leads
{
    public class LeadsGenerator_TrackerOnly : LeadsGeneratorBase
    {
        public override Task MakeFBLead()
        {
            return Task.CompletedTask;
        }
    }
}
