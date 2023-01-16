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
        Task<string> Generate();
        Task Revoke(string link);
    }
}
