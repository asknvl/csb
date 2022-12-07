using csb.server.dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public interface ITGFollowersStatApi
    {
        Task<bool> IsSubscriptionAvailable(string geotag, long id);
        Task UpdateFollowers(List<Follower> followers);
        Task<List<string>> GetFollowerGeoTags(long id);
        Task<List<tgUserDailyPushInfo>> GetUsersNeedDailyPush(string geotag, double hours);
        Task MarkFollowerWasDailyPushed(string geotag, long userId, int pushId, DailyPushState pushState);        
    }

    public enum DailyPushState
    {
        sent,
        delivered,
        disable
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
