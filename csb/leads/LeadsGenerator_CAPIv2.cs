using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace asknvl.leads
{
    public class LeadsGenerator_CAPIv2 : LeadsGeneratorBase
    {
        public LeadsGenerator_CAPIv2(string geotag, ITGFollowerTrackApi trackApi) : base(geotag, trackApi)
        {
        }

        public override Task<string> MakeFBLead(string invite_link, string firstname = null, string lastname = null)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
