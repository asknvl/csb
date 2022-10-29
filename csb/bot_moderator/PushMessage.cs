using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    public class PushMessage
    {
        [JsonProperty]
        public double TimePeriod { get; set; }
        [JsonProperty]
        public TextMessage TextMessage { get; set; }
    }
}
