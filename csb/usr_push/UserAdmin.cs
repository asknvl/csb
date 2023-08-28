
using asknvl.logger;
using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TL;
using WTelegram;

namespace csb.usr_push
{
    public class UserAdmin : TGUserBase
    {
        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
#if DEBUG
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://185.46.9.229:4000");
#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");       
#endif

        CircularBuffer incomeIds = new CircularBuffer(50);
        CircularBuffer outcomeIds = new CircularBuffer(50);
        #endregion

        #region properties        
        #endregion

        public UserAdmin(string api_id, string api_hash, string phone_number, string geotag) : base(api_id, api_hash, phone_number, geotag)
        {
        }

        #region protected
        async Task HandleMessage(MessageBase messageBase)
        {
            long id = 0;
            try
            {
                switch (messageBase)
                {
                    case TL.Message m:

                        id = m.Peer.ID;

                        if (m.flags.HasFlag(TL.Message.Flags.out_))
                        {
                            if (!outcomeIds.ContainsID(id))
                            {
                                await statApi.MarkFollowerWasReplied(geotag, id);
                                outcomeIds.Add(id);
                                logger.inf($"Follower {id} was replied {geotag}");
                                logger.inf_urgent(outcomeIds.ToString());

                            }
                        }
                        else
                        {
                            if (!incomeIds.ContainsID(id))
                            {
                                await statApi.MarkFollowerMadeFeedback(geotag, id);
                                incomeIds.Add(id);
                                logger.inf($"Follower {id} made feedback {geotag}");
                                logger.inf_urgent(incomeIds.ToString());
                            }
                        }
                        break;
                    case MessageService ms:
                        break;
                }
            } catch (Exception ex)
            {
                logger.err($"HandleMessage error: id={id} {ex.Message}");
            }
        }


        protected override async void processUpdates(UpdatesBase updates)
        {
            foreach (var update in updates.UpdateList)
            {

                switch (update)
                {
                    case UpdateNewMessage unm:
                        await HandleMessage(unm.message);
                        break;
                }

            }
        }
        #endregion
    }
}
