using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{

    public class Button {
        [JsonProperty]
        public string? Name { get; set; } = null;
        [JsonProperty]
        public string? Link { get; set; } = null;
    }

    public class GreetingsData
    {
        [JsonProperty]
        public string? HelloMessage { get; set; } = null;
        [JsonProperty]
        public List<Button> Buttons { get; set; } = new();
        [JsonProperty]
        public string? ByeMessage { get; set; } = null;
        [JsonProperty]
        public string? AlternativeLink { get; set; } = null;
    }
}
