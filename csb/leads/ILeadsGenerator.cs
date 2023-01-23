using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace asknvl.leads
{
    public interface ILeadsGenerator
    {
        Task MakeFBLead(ChatInviteLink invite_link);
        Task MakeTrackerLead();
    }

    public enum LeadAlgorithmType : int
    {
        TrackerOnly = 0,
        CAPIv1,
        CAPIv2
    }
}
