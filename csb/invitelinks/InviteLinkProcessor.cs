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
        public Task<string> Generate(ITelegramBotClient bot)
        {
            throw new NotImplementedException();
        }

        public Task Revoke(ITelegramBotClient bot, string link)
        {
            throw new NotImplementedException();
        }
    }
}
