using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace csb.invitelinks
{
    public class InviteLinkProcessor : IInviteLinksProcessor
    {
        #region vars
        ITelegramBotClient bot;
        #endregion

        public InviteLinkProcessor(ITelegramBotClient bot)
        {
            this.bot = bot;
        }

        #region public
        public Task<string> Generate()
        {
            throw new NotImplementedException();
        }

        public Task Revoke(string link)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
