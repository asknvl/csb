using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.leads
{
    public class LeadsGenerator_TrackerOnly : LeadsGeneratorBase
    {
        public LeadsGenerator_TrackerOnly(string geotag, ITGFollowerTrackApi trackApi) : base(geotag, trackApi)
        {
        }

        public override Task MakeFBLead(string invite_link)
        {
            return Task.CompletedTask;
        }
    }
}
