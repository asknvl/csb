using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace csb.invitelinks
{
    public interface IInviteLinksProcessor
    {
        Task<string> Generate(ITelegramBotClient bot);
        Task Revoke(ITelegramBotClient bot, string link);
    }
}
