using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.invitelinks
{
    public class StaticInviteLinkProcessor : IInviteLinksProcessor
    {
        public Task<string> Generate(long? channelid)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<int> Generate(long? channelid, int n)
        {
            return Task.FromResult(0);
        }

        public Task Revoke(long? channelid, string link)
        {
            return Task.CompletedTask;
        }
    }
}
