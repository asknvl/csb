using asknvl.logger;
using csb.server;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace csb.invitelinks
{
    public class DynamicInviteLinkProcessor : IInviteLinksProcessor
    {
        #region vars
        string geotag;        

        ITelegramBotClient bot;
        ITGFollowerTrackApi trackApi;
        ILogger logger;
        #endregion

        public DynamicInviteLinkProcessor(string geotag, ITelegramBotClient bot, ITGFollowerTrackApi trackApi)
        {   
            this.geotag = geotag;            
            this.bot = bot;
            this.trackApi = trackApi; //new TGFollowerTrackApi("https://app.alopadsa.ru");

            logger = new Logger("linkprocessors", geotag);
        }

        #region public
        public async Task<string> Generate(long? channelid)
        {
            string link = "";
            
            logger.inf("Link generate request:");
            if (channelid == null)
            {
                logger.err("Unable to generate link: ChannelID=null");
                return link;
            }

            try
            {
                await Task.Run(async () => {                    

                    var invitelink = await bot.CreateChatInviteLinkAsync(channelid, null, null, null, true);
                    link = invitelink.InviteLink;
                    logger.inf_urgent($"generated:{link}");

                    await trackApi.EnqueueInviteLink(geotag, link);
                    logger.inf("server: enqueued");

                });

            } catch (Exception ex)
            {
                logger.err(ex.Message);
                throw;
            }

            logger.inf($"Link generated {link}");

            return link;
        }

        public async Task<int> Generate(long? channelid, int n)
        {
            logger.inf_urgent($"Link generate request n={n}:");
            if (channelid == null)
            {
                logger.err($"Unable to generate link n={n}: ChannelID=null");
                return 0;
            }

            int res = 0;
            await Task.Run(async () => {

                for (int i = 0; i < n; i++)
                {
                    await Generate(channelid);
                    res++;
                    Thread.Sleep(1000);
                }            
            });

            logger.inf($"Links generated n={n}:");
            return res;
        }

        public async Task Revoke(long? channelid, string link)
        {
            logger.inf($"Link revoke request {link}");
            if (channelid == null)
            {
                logger.err("Unable to revoke link: ChannelID=null");
                return;
            }

            try
            {
                await bot.RevokeChatInviteLinkAsync(channelid, link);
                logger.inf_urgent($"revoked: CH={channelid} link={link}");
            } catch (Exception ex) {

                logger.err(ex.Message);
                throw;
            }
        }
        #endregion
    }
}
