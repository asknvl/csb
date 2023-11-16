using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace asknvl.leads
{
    public class LeadsGenerator_TrackerOnly : LeadsGeneratorBase
    {
        public LeadsGenerator_TrackerOnly(string geotag, ITGFollowerTrackApi trackApi) : base(geotag, trackApi)
        {
        }

        public override Task<string> MakeFbOptimizationEvent(string invite_link, string firstname = null, string lastname = null)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
