using System;
using System.Collections.Generic;
using csb.messaging;
using Newtonsoft.Json;

namespace csb.bot_moderator
{
    public class DailyPushData
    {
        [JsonProperty]
        public List<DailyPushMessage> Messages { get; set; } = new();
    }
}
