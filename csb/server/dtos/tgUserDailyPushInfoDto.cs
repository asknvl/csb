using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server.dtos
{
    public class tgUserDailyPushInfoDto
    {
        public string tg_user_id { get; set; }
        public int? notification_delivered_id { get; set; }
    }
}
