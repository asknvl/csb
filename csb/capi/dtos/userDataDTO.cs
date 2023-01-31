using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace capi_test.capi.dtos
{
    public class userDataDTO
    {
        //[JsonProperty]
        //public string external_id { get; set; }
        [JsonProperty]
        public string fn { get; set; }
        [JsonProperty]
        public string ln { get; set; }
        [JsonProperty]
        public string client_user_agent { get; set; }
        [JsonProperty]
        public string client_ip_address { get; set; }
        [JsonProperty]
        public string fbc { get; set; }
    }
}
