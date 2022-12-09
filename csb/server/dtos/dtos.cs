using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server.dtos
{
    #region MarkFollowerWasDailyPushed
    public class tgUserDailyPushSentDto
    {
        public long tg_user_id { get; set; }
        public string tg_geolocation { get; set; }
        public int notification_send_id { get; set; }
    }

    public class tgUsersDailyPushSentDto
    {
        public List<tgUserDailyPushSentDto> users { get; set; } = new();
    }

    public class tgUserDailyPushDeliveredDto
    {
        public long tg_user_id { get; set; }
        public string tg_geolocation { get; set; }
        public int notification_delivered_id { get; set; }
    }

    public class tgUsersDailyPushDeliveredDto
    {
        public List<tgUserDailyPushDeliveredDto> users { get; set; } = new();
    }

    public class tgUserDailyPushDisableDto
    {
        public long tg_user_id { get; set; }
        public string tg_geolocation { get; set; }
        public bool notification_enabled { get; set; }
    }

    public class tgUsersDailyPushDisableDto
    {
        public List<tgUserDailyPushDisableDto> users { get; set; } = new();
    }
    #endregion

    #region GetUsersNeedDailyPush
    public class tgUserDailyPushInfo
    {
        public string tg_user_id { get; set; }
        public int? notification_delivered_id { get; set; }
    }
    public class tgUserDailyPushResultDto
    {
        public bool success { get; set; }
        public int count_telegram_users { get; set; }
        public string geo { get; set; }
        public List<tgUserDailyPushInfo> data { get; set; }
    }
    #endregion

    #region GetFollowerGeoTags
    class geoTagDto
    {
        public int id { get; set; }
        public string code { get; set; }
    }
    class tgUserDto
    {
        public string tg_user_id { get; set; }
        public List<geoTagDto> geolocations { get; set; }
    }
    #endregion

    #region IsSubscriptionAvailable
    class subAvaliableData
    {
        public bool is_available { get; set; }
        public int group_id { get; set; }
    }
    class subAvaliableResult
    {
        public bool success { get; set; }
        public subAvaliableData data { get; set; }
    }
    #endregion

    #region UpdateFollowers
    public class followersDto
    {
        public List<Follower> users { get; set; }
        public followersDto(List<Follower> followers)
        {
            this.users = followers;
        }
    }
    #endregion
}
