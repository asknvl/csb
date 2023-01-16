using asknvl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.leads
{
    public abstract class LeadsGeneratorBase
    {
        public abstract Task MakeFBLead();

        public virtual Task MakeTrackerLead()
        {
            return Task.CompletedTask;
        }
    }
}
