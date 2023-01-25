using capi_test.capi;
using csb.server;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace asknvl.leads
{
    public class LeadsGenerator_CAPIv1 : LeadsGeneratorBase
    {
        #region vars
        ICAPI capi;
        #endregion

        public LeadsGenerator_CAPIv1(string geotag, ITGFollowerTrackApi trackApi) : base(geotag, trackApi)
        {
            capi = new CAPI();
        }
        #region public
        public override async Task MakeFBLead(string invite_link, string firstname = null, string lastname = null)
        {
            var lead_data = await trackApi.GetLeadData(invite_link);

            await capi.MakeLeadEvent(
                        lead_data.fb_pixel,
                        lead_data.fb_capi,
                        tg_user_id:null,
                        firstname: firstname,
                        lastname: lastname,
                        client_user_agent:lead_data.user_agent,
                        client_ip_address:lead_data.ip
                    );
        }
        #endregion
    }
}
