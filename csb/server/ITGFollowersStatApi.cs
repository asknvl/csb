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
        Task<List<tgUserDailyPushInfoDto>> GetUsersNeedDailyPush(string geotag, double hours);
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
