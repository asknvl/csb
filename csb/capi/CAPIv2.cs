using capi_test.capi;
using capi_test.capi.dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace csb.capi
{
    public class CAPIv2 : CAPI
    {
        override public async Task<string> MakeContactEvent(string pixel_id,
                                       string token,
                                       long? tg_user_id = null,
                                       string firstname = null,
                                       string lastname = null,
                                       string client_user_agent = null,
                                       string client_ip_address = null,
                                       string fbc = null,
                                       string fbp = null,
                                       string test_event_code = null)
        {
            var path = $"https://graph.facebook.com/{API_VERSION}/{pixel_id}/events?access_token={token}";
            var httpClient = httpClientFactory.CreateClient();

            long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            serverEventDTO serverEvent = new serverEventDTO()
            {
                action_source = "website",
                event_name = "Contact",
                event_time = time,

                user_data = new userDataDTO()
                {
                    //external_id = $"{tg_user_id}",
                    fn = (!string.IsNullOrEmpty(firstname)) ? getSHA256(firstname) : null,
                    ln = (!string.IsNullOrEmpty(lastname)) ? getSHA256(lastname) : null,
                    client_user_agent = (!string.IsNullOrEmpty(client_user_agent)) ? client_user_agent : null,
                    client_ip_address = (!string.IsNullOrEmpty(client_ip_address)) ? client_ip_address : null,
                    fbc = (!string.IsNullOrEmpty(fbc)) ? $"fb.1.{time}.{fbc}" : null,
                    fbp = (!string.IsNullOrEmpty(fbp)) ? $"{fbp}" : null
                }


            };

            string json;

            if (test_event_code == null)
            {
                serverEventsDTO events = new serverEventsDTO()
                {
                    data = new List<serverEventDTO>()
                    {
                        serverEvent
                    }
                };
                json = JsonConvert.SerializeObject(events);
            }
            else
            {
                testServerEventsDTO events = new testServerEventsDTO()
                {
                    data = new List<serverEventDTO>()
                    {
                        serverEvent
                    },

                    test_event_code = test_event_code
                };
                json = JsonConvert.SerializeObject(events);
            }

            var data = new StringContent(json, Encoding.UTF8, "application/json");
            string result = "";

            try
            {
                var response = await httpClient.PostAsync(path, data);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"MakeLeadEvent {ex.Message}");
            }

            return result;
        }
    }
}
