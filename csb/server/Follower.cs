using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public class Follower : IFollower
    {
        public long tg_user_id { get; set; }
        public long tg_chat_id { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string invite_link { get; set; }
        public string tg_geolocation { get; set; }
    }
}
