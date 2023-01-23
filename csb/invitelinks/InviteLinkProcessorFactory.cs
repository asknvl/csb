using asknvl.leads;
using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace csb.invitelinks
{
    public class InviteLinkProcessorFactory
    {
        public static IInviteLinksProcessor Create(string geotag, LeadAlgorithmType? type,
                                                   ITelegramBotClient bot,
                                                   ITGFollowerTrackApi trackApi)
        {
            switch (type)
            {
                default:
                case null:
                case LeadAlgorithmType.TrackerOnly:
                    return new StaticInviteLinkProcessor();                    
                case LeadAlgorithmType.CAPIv1:
                case LeadAlgorithmType.CAPIv2:
                    return new DynamicInviteLinkProcessor(geotag, bot, trackApi);                

            }
        }
    }
}
