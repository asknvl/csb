using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        //[JsonIgnore]
        public bool is_subscribed { get; set; }

        public override string ToString()
        {

            var status = (is_subscribed) ? "JOIN" : "LEFT";

            string usrnm = !string.IsNullOrEmpty(username) ? $"@{username}" : "";

            return $"{DateTime.Now} " +
                   $"{tg_geolocation} " +
                   $"{status} " +
                   $"{tg_chat_id} " +
                   $"{tg_user_id} " +
                   $"{firstname} " +
                   $"{lastname} " +
                   $"{usrnm}";

        }
    }
}
