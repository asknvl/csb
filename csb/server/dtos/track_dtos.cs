using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server.track_dtos
{
    #region EnqueueLink
    public class InviteLinkDto
    {
        [JsonProperty]
        public string geolocation { get; set; }
        [JsonProperty]
        public string invite_link { get; set; }
    }
    #endregion

    #region GetLeadData
    public class linkParameterDto
    {
        [JsonProperty]
        public string link { get; set; }
    }
    public class leadDataDtoResponse
    {
        [JsonProperty]
        public bool success { get; set; }
        [JsonProperty]
        public leadDataDto data { get; set; }
    }
    public class leadDataDto
    {
        [JsonProperty]
        public int id { get; set; }
        [JsonProperty]
        public string tg_link { get; set; }
        [JsonProperty]
        public string tr_subid { get; set; }
        [JsonProperty]
        public string tr_campaign_ig { get; set; }
        [JsonProperty]
        public string tr_campaign_name { get; set; }
        [JsonProperty]
        public string fbcl_id { get; set; }
        [JsonProperty]
        public string fb_capi { get; set; }
        [JsonProperty]
        public string fb_pixel { get; set; }
        [JsonProperty]
        public string fb_ad_campaign_id { get; set; }
        [JsonProperty]
        public string fb_creative_capi { get; set; }
        [JsonProperty]
        public string fingerptint_visitor_id { get; set; }
        [JsonProperty]
        public string ip { get; set; }
        [JsonProperty]
        public string country { get; set; }
        [JsonProperty]
        public string user_agent { get; set; }
    }
    #endregion
}
