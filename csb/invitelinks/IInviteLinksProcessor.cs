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
        Task<string> Generate(long? channelid);
        Task<int> Generate(long? channelid, int n);
        Task Revoke(long? channelid, string link);
    }
}
