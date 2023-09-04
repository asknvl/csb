using csb.server.tg_dtos;
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
        Task MarkFollowerWasDeclined(string geotag, long id);
        Task<List<tgUserPushInfoDto>> GetNoFeedbackFollowers(string geotag, string date_from, string date_to);
        Task MarkFollowerWasPushed(string geotag, long id, double hours, bool status);
        Task<List<string>> GetFollowerGeoTags(long id);
        Task<List<tgUserDailyPushInfo>> GetUsersNeedDailyPush(string geotag, double hours);
        Task MarkFollowerWasDailyPushed(string geotag, long userId, int pushId, DailyPushState pushState);
        Task MarkFollowerMadeFeedback(string geotag, long id);
        Task MarkFollowerWasReplied(string geotag, long id);
        Task<List<long>> GetUsersNeedAutoReply(string geotag, int minute_offset, double minute_period);
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
