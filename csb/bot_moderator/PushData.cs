using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    public class PushData
    {
        [JsonProperty]
        public List<PushMessage> Messages { get; set; } = new();
    }
}
