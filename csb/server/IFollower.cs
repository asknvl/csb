using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public interface IFollower
    {
        long tg_user_id { get; set; }
        long tg_chat_id { get; set; }   
        string username { get; set; }
        string firstname { get; set; }
        string lastname { get; set; }   
        string invite_link { get; set; }
        string tg_geolocation { get; set; }
        string subscribe_date { get; set; }
    }
}
