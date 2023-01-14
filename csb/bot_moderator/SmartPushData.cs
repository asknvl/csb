using csb.messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    public class SmartPushData
    {
        [JsonProperty]
        public List<SmartPushMessage> Messages { get; set; } = new();
    }
}
