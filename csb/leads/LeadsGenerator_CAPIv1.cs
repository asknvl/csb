using csb.server;
using System.Threading.Tasks;

namespace asknvl.leads
{
    public class LeadsGenerator_CAPIv1 : LeadsGeneratorBase
    {
        public LeadsGenerator_CAPIv1(string geotag, ITGFollowerTrackApi trackApi) : base(geotag, trackApi)
        {
        }

        public override async Task MakeFBLead(string invite_link)
        {
            await Task.Run(() => {
                var lead_data = trackApi.GetLeadData(invite_link);
            });
        }
    }
}
