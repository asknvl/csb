using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace capi_test.capi.dtos
{
    public class serverEventDTO
    {
        [JsonProperty]
        public string event_name { get; set; }
        [JsonProperty]
        public long event_time { get; set; }
        [JsonProperty]
        public userDataDTO user_data { get; set; }
        [JsonProperty]
        public string action_source { get; set; }   
    }
}
