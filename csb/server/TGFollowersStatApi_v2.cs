using csb.server.dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public class TGFollowersStatApi_v2 : ITGFollowersStatApi
    {
        public Task<List<tgUserDailyPushInfoDto>> GetUsersNeedDailyPush(string geotag, double hours)
        {
            throw new NotImplementedException();
        }
    }
}
