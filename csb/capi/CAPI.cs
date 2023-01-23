using capi_test.capi.dtos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace capi_test.capi
{
    public class CAPI : ICAPI
    {
        #region const
        string API_VERSION = "v15.0";
        #endregion

        #region vars
        string path = "";
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        #endregion

        public CAPI()
        {
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        }

        #region private
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }
        #endregion

        #region private
        string getSHA256(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using (var hashEngine = SHA256.Create())
            {
                var hashedBytes = hashEngine.ComputeHash(bytes, 0, bytes.Length);
                var sb = new StringBuilder();
                foreach (var b in hashedBytes)
                {
                    var hex = b.ToString("x2");
                    sb.Append(hex);
                }
                return sb.ToString();
            }
        }
        #endregion

        #region public     
        public async Task MakeLeadEvent(string pixel_id,
                                        string token,
                                        long? tg_user_id = null,
                                        string firstname = null,
                                        string lastname = null,
                                        string client_user_agernt = null,
                                        string client_ip_address = null,
                                        string test_event_code = null)
        {
            path = $"https://graph.facebook.com/{API_VERSION}/{pixel_id}/events?access_token={token}";
            var httpClient = httpClientFactory.CreateClient();

            serverEventDTO serverEvent = new serverEventDTO()
            {
                action_source = "chat",
                event_name = "Lead",
                event_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                user_data = new userDataDTO()
                {
                    //external_id = $"{tg_user_id}",
                    fn = (!string.IsNullOrEmpty(firstname)) ? getSHA256(firstname.ToLower()) : null,
                    ln = (!string.IsNullOrEmpty(lastname)) ? getSHA256(lastname.ToLower()) : null,
                    client_user_agent = (!string.IsNullOrEmpty(client_user_agernt)) ? client_user_agernt : null,
                    client_ip_address = (!string.IsNullOrEmpty(client_ip_address)) ? client_ip_address : null                    
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
            } else
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

            try
            {
                var response = await httpClient.PostAsync(path, data);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine(result);

            } catch (Exception ex)
            {
                throw new Exception($"MakeEvent {ex.Message}");
            }
        }
        #endregion
    }
}
