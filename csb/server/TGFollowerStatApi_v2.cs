﻿using csb.server.dtos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public class TGFollowersStatApi_v2 : ITGFollowersStatApi
    {
        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        #endregion

        public TGFollowersStatApi_v2(string url)
        {
            this.url = url;

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

        public async Task UpdateFollowers(List<Follower> followers)
        {
            var addr = $"{url}/v1/telegram";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            followersDto flwrs = new followersDto(followers);
            var json = JsonConvert.SerializeObject(flwrs);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            } catch (Exception ex)
            {
                throw new Exception($"UpdateFollowers {ex.Message}");
            }

        }

        public virtual async Task<List<tgUserPushInfoDto>> GetNoFeedbackFollowers(string geotag, string date_from, string date_to)
        {
            List<tgUserPushInfoDto> users = new();

            var addr = $"{url}/v1/telegram/usersWithoutFeedback?date_from={date_from}&date_to={date_to}&geo={geotag}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<tgUsersPushResultDto>(result);

                if (resp.success)
                    users = resp.data;
                else
                    throw new Exception($"syccess=false");
            } catch (Exception ex)
            {
                throw new Exception($"GetNoFeedbackFollowers {ex.Message}");
            }

            return users;
        }

        public virtual async Task MarkFollowerMadeFeedback(string geotag, long id)
        {

            tgUsersFeedbackDto feedback = new();
            feedback.users.Add(new tgUserFeedbackDto()
            {
                tg_user_id = id,
                tg_geolocation = geotag,
                is_user_send_msg = true
            });

            string json = JsonConvert.SerializeObject(feedback);

            var addr = $"{url}/v1/telegram/userByGeo";
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            } catch (Exception ex)
            {
                throw new Exception($"MarkFollowerMadeFeedback {ex.Message}");
            }

        }

        public virtual async Task MarkFollowerWasPushed(string geotag, long id, double hours, bool status)
        {
            string json;

            if (status)
            {
                tgUsersPushesDeliveredDto delivered = new();
                delivered.users.Add(new tgUserPushDeliveredDto()
                {
                    tg_user_id = id,
                    tg_geolocation = geotag,
                    push_delivered_hours = hours
                });
                json = JsonConvert.SerializeObject(delivered);
                
            } else
            {
                tgUsersPushesSentDto sent = new();
                sent.users.Add(new tgUserPushSentDto()
                {
                    tg_user_id = id,
                    tg_geolocation = geotag,
                    push_send_hours = hours
                });

                json = JsonConvert.SerializeObject(sent);
                
            }

            var addr = $"{url}/v1/telegram/userByGeo";
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            } catch (Exception ex)
            {
                throw new Exception($"MarkFollowerWasPushed {ex.Message}");
            }

        }

        public async Task<bool> IsSubscriptionAvailable(string geotag, long id)
        {
            bool res = false;

            var addr = $"{url}/v1/telegram/subscriptionAvailability?geo={geotag}&userID={id}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<subAvaliableResult>(result);

                if (resp.success)
                    res = resp.data.is_available;
                else
                    throw new Exception($"sucess=false");
            } catch (Exception ex)
            {
                throw new Exception($"IsSubscriptionAvaliable {ex.Message}");
            }

            return res;
        }

        public async Task<List<string>> GetFollowerGeoTags(long id)
        {
            List<string> tags = new List<string>();

            var addr = $"{url}/v1/telegram/userByID/{id}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(result);
                bool res = json["success"].ToObject<bool>();
                if (res)
                {
                    var data = json["telegramUser"];
                    if (data != null)
                    {
                        var d = data.ToObject<tgUserDto>();
                        tags = d.geolocations.Select(g => g.code).ToList();
                    }

                } else
                {
                    throw new TGFollowersStatException($"success=false");
                }

            } catch (Exception ex)
            {
                throw new Exception($"GetFollowerGeoTags {ex.Message}");
            }

            return tags;
        }
        #endregion

        #region public
        public async Task<List<tgUserDailyPushInfo>> GetUsersNeedDailyPush(string geotag, double hours)
        {
            List<tgUserDailyPushInfo> users = new();

            var addr = $"{url}/v1/telegram/usersNotification?min_hours_after_last_push={hours}&geo={geotag}";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<tgUserDailyPushResultDto>(result);
                if (resp.success)
                    users = resp.data;
                else
                    throw new Exception("success=false");

            } catch (Exception ex)
            {
                throw new Exception($"GetUsersNeedDailyPush {ex.Message}");
            }

            return users;
        }

        public virtual async Task MarkFollowerWasDailyPushed(string geotag, long userId, int pushId, DailyPushState pushState)
        {
            string json = "";

            switch (pushState)
            {
                case DailyPushState.sent:
                    tgUsersDailyPushSentDto sent = new();
                    sent.users.Add(new tgUserDailyPushSentDto()
                    {
                        tg_geolocation = geotag,
                        tg_user_id = userId,
                        notification_send_id = pushId
                    });
                    json = JsonConvert.SerializeObject(sent);
                    break;
                case DailyPushState.delivered:
                    tgUsersDailyPushDeliveredDto delivered = new();
                    delivered.users.Add(new tgUserDailyPushDeliveredDto()
                    {
                        tg_geolocation = geotag,
                        tg_user_id = userId,
                        notification_delivered_id = pushId
                    });
                    json = JsonConvert.SerializeObject(delivered);
                    break;
                case DailyPushState.disable:
                    tgUsersDailyPushDisableDto disable = new();
                    disable.users.Add(new tgUserDailyPushDisableDto()
                    {
                        tg_geolocation = geotag,
                        tg_user_id = userId,
                        notification_enabled = false
                    });
                    json = JsonConvert.SerializeObject(disable);
                    break;
                default:
                    throw new Exception("MarkFolloweWasDailyPushed Unknown DailyPushState");
            }

            var addr = $"{url}/v1/telegram/userByGeo";
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.PostAsync(addr, data);
                var result = await response.Content.ReadAsStringAsync();
                var jres = JObject.Parse(result);
                bool res = jres["success"].ToObject<bool>();
                if (!res)
                    throw new Exception($"success=false");

            } catch (Exception ex)
            {
                throw new Exception($"MarkFolloweWasDailyPushed {ex.Message}");
            }

        }
        #endregion
    }
}