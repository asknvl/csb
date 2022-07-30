using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server.serialization
{
    public class sfollowers
    {
        [JsonProperty("users")]
        public List<Follower> users { get; set; }

        public sfollowers(List<Follower> followers)
        {
            this.users = followers;
        }
    }
}
