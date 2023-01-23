using asknvl.logger;
using csb.server;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace asknvl.leads
{
    public abstract class LeadsGeneratorBase : ILeadsGenerator
    {
        #region vars
        protected string GeoTag;
        protected ITGFollowerTrackApi trackApi;
        protected ILogger logger;        
        #endregion

        public LeadsGeneratorBase(string geotag, ITGFollowerTrackApi trackApi)
        {
            GeoTag = geotag;
            this.trackApi = trackApi;            
        }

        public abstract Task MakeFBLead(ChatInviteLink invite_link);

        public virtual Task MakeTrackerLead()
        {
            return Task.CompletedTask;
        }
    }
}
