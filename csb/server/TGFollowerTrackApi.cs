using csb.server.tg_dtos;
using csb.server.track_dtos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace csb.server
{    
    public class TGFollowerTrackApi : ITGFollowerTrackApi
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;        
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        #endregion

        public TGFollowerTrackApi(string url)
        {
            this.url = url;
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        }

        #region public
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }

        public async Task EnqueueInviteLink(string geotag, string invite_link)
        {
            var addr = $"{url}/v1/telegram/telegramLink";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            InviteLinkDto param = new InviteLinkDto()
            {
                geolocation = geotag,
                invite_link = invite_link
            };

            var json = JsonConvert.SerializeObject(param);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception("success=false");

            } catch (Exception ex)
            {
                throw new Exception($"EnqueueInviteLink {ex.Message}");
            }
        }

        public async Task<int> GetInviteLinksAvailable(string geotag)
        {

            int res = 0;

            var addr = $"{url}/v1/telegram/availableTelegramLinks?geo={geotag}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<availableInviteLinksDtoResponse>(result);

                if (resp.success)
                    res = resp.available_count;
                else
                    throw new Exception($"success=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetInviteLinksAvailabl {ex.Message}");
            }

            return res;
        }

        public async Task<leadDataDto> GetLeadData(string link)
        {
            leadDataDto leadData = new();

            var addr = $"{url}/v1/telegram/clientDataByLink";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            linkParameterDto jlink = new linkParameterDto()
            {
                link = link
            };
            var json = JsonConvert.SerializeObject(jlink);
            var data = new StringContent(json, Encoding.UTF8, "application/json");


            try
            {
                var response = await httpClient.PostAsync(addr, data);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<leadDataDtoResponse>(result);

                if (resp.success)
                    leadData = resp.data;
                else
                    throw new Exception($"success=false");
            } catch (Exception ex)
            {
                throw new Exception($"GetLeadData {ex.Message}");
            }

            return leadData;
        }
        #endregion
    }
}
