using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.leads
{
    public interface ILeadsGenerator
    {
        Task MakeFBLead();
        Task MakeTrackerLead();
    }

    public enum LeadAlgorithmType : int
    {
        TrackerOnly = 0,
        CAPIv1,
        CAPIv2
    }
}
