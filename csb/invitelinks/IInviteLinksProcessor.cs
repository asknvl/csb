using System;
using System.Threading;
using System.Threading.Tasks;

namespace csb.invitelinks
{
    public interface IInviteLinksProcessor
    {
        Task<string> Generate(long? channelid);
        //Task<int> Generate(long? channelid, int n);
        Task Revoke(long? channelid, string link);
        Task StartLinkNumberControl(long? channelid, CancellationTokenSource cts);
        void UpdateChannelID(long? channelid);

        event Action<string> ExceptionEvent;
    }
}
