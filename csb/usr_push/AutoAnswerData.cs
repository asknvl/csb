using csb.messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public class AutoAnswerData
    {
        [JsonProperty]
        public List<AutoAnswerMessage> Messages { get; set; } = new();
    }
}
