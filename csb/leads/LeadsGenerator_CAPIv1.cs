using asknvl.logger;
using capi_test.capi;
using csb.server;
using System;
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
            logger = new Logger("leadsgenerator_CAPIv1", geotag);
        }
        #region public
        public override async Task<string> MakeFBLead(string invite_link, string firstname = null, string lastname = null)
        {
            logger.inf_urgent("lead_data?");
            var lead_data = await trackApi.GetLeadData(invite_link);
            logger.inf_urgent($"lead_data: $link={lead_data.tg_link}\n" +
                                           $"pixel={lead_data.fb_pixel}\n" +
                                           $"capi={lead_data.fb_capi}\n" +
                                           $"fbc={lead_data.fbcl_id}\n" +
                                           $"ip={lead_data.ip}\n" +
                                           $"ua={lead_data.user_agent}");

            try
            {
                var leadRes = await capi.MakeLeadEvent(
                                lead_data.fb_pixel,
                                lead_data.fb_capi,
                                tg_user_id: null,
                                firstname: firstname,
                                lastname: lastname,
                                client_user_agent: lead_data.user_agent,
                                client_ip_address: lead_data.ip);

                logger.inf_urgent(leadRes);

            } catch (Exception ex)
            {
                logger.err(ex.Message);
            }

            return lead_data.tg_link;
        }
        #endregion
    }
}
