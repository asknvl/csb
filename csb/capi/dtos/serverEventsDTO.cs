using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace capi_test.capi.dtos
{
    public class serverEventsDTO
    {
        [JsonProperty]
        public List<serverEventDTO> data = new();
    }

    public class testServerEventsDTO
    {
        [JsonProperty]
        public List<serverEventDTO> data = new();
        [JsonProperty]
        public string test_event_code { get; set; }
    }
}
