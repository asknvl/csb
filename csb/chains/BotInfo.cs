using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.chains
{
    public class BotInfo
    {
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public long ChannelID { get; set; }
        [JsonProperty]
        public string ChannelTitle { get; set; }
        [JsonProperty]
        public string ChannelLink { get; set; }

        public override string ToString()
        {
            return $"{Name}:\n{Token}\n{ChannelTitle}\n{ChannelLink}";
        }
    }
}
