using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    public class GreetingsData
    {
        [JsonProperty]
        public string? HelloMessage { get; set; } = null;
        [JsonProperty]
        public string? ReisterLink { get; set; } = null;
        [JsonProperty]
        public string? MessageMeLink { get; set; } = null;
        [JsonProperty]
        public string? ChannelOpenLink { get; set; } = null;
        [JsonProperty]
        public string? AlternativeLink { get; set; } = null;
    }
}
