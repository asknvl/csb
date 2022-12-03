using csb.server.serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public abstract class TGFollowersStatApi
    {

        #region const
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjksImxldHRlcl9pZCI6IllCIiwiaWF0IjoxNjU5MTk2Nzc1fQ.8qzVaYVky9m4m3aa0f8mMFI6mk3-wyhAiSZVmiHKwmg";
        #endregion

        #region vars
        string url;
        #endregion

        public TGFollowersStatApi(string url)
        {
            this.url = url; 
        }

        #region public
        public virtual async Task UpdateFollowers(List<Follower> followers)
        {
            try
            {
                await Task.Run(() => {
                    var client = new RestClient($"{url}/v1/telegram");
                    var request = new RestRequest(Method.POST);
                    request.AddHeader($"Authorization", $"Bearer {token}");

                    sfollowers sf = new sfollowers(followers);
                    string jfollowers = JsonConvert.SerializeObject(sf);

                    request.AddParameter("application/json", jfollowers, ParameterType.RequestBody);

                    var response = client.Execute(request);
                    var json = JObject.Parse(response.Content);
                    var res = json["success"].ToObject<bool>();
                    if (!res)
                        throw new TGFollowersStatException("Не удалось зарегестрировать подписчика");                    
                        
                });

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task<int> GetFollowersNumber(string geotag, string startDate, string endDate)
        {
            int followersNumber = 0;
            string followersGeo = "";

            try
            {
                await Task.Run(() =>
                {

                    var client = new RestClient($"{url}/v1/telegram/userCount");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader($"Authorization", $"Bearer {token}");
                    dynamic p = new JObject();
                    p.date_from = startDate;
                    p.date_to = endDate;
                    p.geo = geotag;
                    request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
                    var response = client.Execute(request);
                    var json = JObject.Parse(response.Content);
                    bool res = json["success"].ToObject<bool>();
                    if (res)
                    {
                        JToken data = json["data"];
                        if (data != null)
                        {
                            followersNumber = data["count_telegram_users"].ToObject<int>();
                            followersGeo = data["geo"].ToString();
                        }

                    } else
                    {
                        throw new TGFollowersStatException("Не удалось получить количество подписчиков");
                    }

                });

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return followersNumber;
        }


        class geoTagDto
        {
            public int id { get; set; }
            public string code { get; set; }
        }

        class tgUserDto
        {
            public string tg_user_id { get; set; } 
            public List<geoTagDto> geolocations { get; set; }
        }

        public virtual async Task<List<string>> GetFollowerGeoTags(long id)
        {
            List<string> tags = new List<string>();

            try
            {           
                var client = new RestClient($"{url}/v1/telegram/userByID/{id}");
                var request = new RestRequest(Method.GET);
                request.AddHeader($"Authorization", $"Bearer {token}");
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
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
                    throw new TGFollowersStatException($"Не удалось получить геотеги для id={id}");
                }

            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return tags;
        }


        public class tgUserPushInfoDto
        {
            public string tg_user_id { get; set; }
            public string tg_chat_id { get;set; }
            public double? push_send_hours { get; set; }
            public double? push_delivered_hours { get; set; }
            public double time_after_subscribe { get; set; }
            public double time_diff_last_push_subscr { get; set; }  
        }
        public class tgUsersPushResultDto
        {
            public bool success { get; set; }
            public string geo { get; set; }
            public List<tgUserPushInfoDto> data { get; set; } = new();
        }

        public virtual async Task<List<tgUserPushInfoDto>> GetNoFeedbackFollowers(string geotag, string date_from, string date_to)
        {

            List<tgUserPushInfoDto> users = new();

            await Task.Run(() =>
            {
                var client = new RestClient($"{url}/v1/telegram/usersWithoutFeedback?date_from={date_from}&date_to={date_to}&geo={geotag}");
                var request = new RestRequest(Method.GET);
                request.AddHeader($"Authorization", $"Bearer {token}");
                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resp = JsonConvert.DeserializeObject<tgUsersPushResultDto>(response.Content);
                    if (resp.success)
                        users = resp.data;
                    else
                        throw new Exception($"GetNoFeedBackUsers success={resp.success}");

                } else
                    throw new Exception($"Не удалось получить информацию о фидбеках подписчиков geotag={geotag}");
            });

            return users;
        }

        public class tgUserFeedbackDto
        {
            public long tg_user_id { get; set; }
            public string tg_geolocation { get; set; }  
            public bool is_user_send_msg { get; set; }  
        }
        public class tgUsersFeedbackDto
        {
            public List<tgUserFeedbackDto> users { get; set; } = new();
        }
        public virtual async Task MarkFollowerMadeFeedback(string geotag, long id)
        {
            await Task.Run(() => {

                var client = new RestClient($"{url}/v1/telegram/userByGeo");
                var request = new RestRequest(Method.POST);
                request.AddHeader($"Authorization", $"Bearer {token}");

                tgUsersFeedbackDto feedback = new();
                feedback.users.Add(new tgUserFeedbackDto() { 
                    tg_user_id = id,
                    tg_geolocation = geotag,
                    is_user_send_msg = true
                });

                string jfeedback = JsonConvert.SerializeObject(feedback);
                request.AddParameter("application/json", jfeedback, ParameterType.RequestBody);

                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = JObject.Parse(response.Content);
                    bool res = json["success"].ToObject<bool>();
                    if (!res)
                        throw new Exception($"MarkFollowerMadeFeedback success={res}");

                } else
                    throw new Exception($"Не удалось пометить фидбэк подписчика code={response.StatusCode} text={response.Content}");

            });
        }

        public class tgUserPushSentDto
        {
            public long tg_user_id { get; set;}
            public string tg_geolocation { get; set; }
            public double push_send_hours { get; set; }
        }

        public class tgUsersPushesSentDto
        {
            public List<tgUserPushSentDto> users { get; set; } = new();
        }

        public class tgUserPushDeliveredDto
        {
            public long tg_user_id { get; set;}
            public string tg_geolocation { get; set; }
            public double push_delivered_hours { get; set; }
        }

        public class tgUsersPushesDeliveredDto
        {
            public List <tgUserPushDeliveredDto> users { get; set; } = new();
        }

        public virtual async Task MarkFollowerWasPushed(string geotag, long id, double hours, bool result)
        {
            await Task.Run(() => {

                var client = new RestClient($"{url}/v1/telegram/userByGeo");
                var request = new RestRequest(Method.POST);
                request.AddHeader($"Authorization", $"Bearer {token}");

                if (result)
                {
                    tgUsersPushesDeliveredDto delivered = new();
                    delivered.users.Add(new tgUserPushDeliveredDto()
                    {
                        tg_user_id = id,
                        tg_geolocation = geotag,
                        push_delivered_hours = hours
                    });
                    string jdelivered = JsonConvert.SerializeObject(delivered);
                    request.AddParameter("application/json", jdelivered, ParameterType.RequestBody);

                } else
                {
                    tgUsersPushesSentDto sent = new();
                    sent.users.Add(new tgUserPushSentDto()
                    {
                        tg_user_id = id,
                        tg_geolocation = geotag,
                        push_send_hours = hours
                    });
                    string jsent = JsonConvert.SerializeObject(sent);
                    request.AddParameter("application/json", jsent, ParameterType.RequestBody);
                }

                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = JObject.Parse(response.Content);
                    bool res = json["success"].ToObject<bool>();
                    if (!res)
                        throw new Exception($"MarkFollowerWasPushed success={res}");

                } else
                    throw new Exception($"Не удалось пометить результат push подписчика");
            });
        }

        public class tgUserDailyPushInfo
        {
            public string tg_user_id { get; set; }
            public int? notification_delivered_id { get; set; } 
        }

        public class tgUserDailyPushResultDto
        {
            public bool success { get; set; }
            public int count_telegram_users { get; set; }
            public string geo { get; set; }
            public List<tgUserDailyPushInfo> data { get; set; }
        }

        public virtual async Task<List<tgUserDailyPushInfo>> GetUsersNeedDailyPush(string geotag, double hours)
        {
            List<tgUserDailyPushInfo> users = new();
            await Task.Run(() =>
            {
                var client = new RestClient($"{url}/v1/telegram/usersNotification?min_hours_after_last_push={hours}&geo={geotag}");
                var request = new RestRequest(Method.GET);
                request.AddHeader($"Authorization", $"Bearer {token}");
                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resp = JsonConvert.DeserializeObject<tgUserDailyPushResultDto>(response.Content);
                    if (resp.success)
                        users = resp.data;
                    else
                        throw new Exception($"GetUsersNeedDailyPush success={resp.success}");

                }
                else
                    throw new Exception($"Не удалось получить список подписчиков для ежедневного {hours} пуш сообщения geotag={geotag}");
            });

            return users;
        }

        public enum DailyPushState
        {
            sent,
            delivered,
            disable
        }

        public class tgUserDailyPushSentDto
        {
            public long tg_user_id { get; set; }
            public string tg_geolocation { get; set; }
            public int notification_send_id { get; set; }
        }

        public class tgUsersDailyPushSentDto
        {
            public List<tgUserDailyPushSentDto> users { get; set; } = new();
        }

        public class tgUserDailyPushDeliveredDto
        {
            public long tg_user_id { get; set; }
            public string tg_geolocation { get; set; }
            public int notification_delivered_id { get; set; }
        }

        public class tgUsersDailyPushDeliveredDto
        {
            public List<tgUserDailyPushDeliveredDto> users { get; set; } = new();
        }

        public class tgUserDailyPushDisableDto
        {
            public long tg_user_id { get; set; }
            public string tg_geolocation { get; set; }
            public bool notification_enabled { get; set; }
        }

        public class tgUsersDailyPushDisableDto
        {
            public List<tgUserDailyPushDisableDto> users { get; set; } = new();
        }

        public virtual async Task MarkFollowerWasDailyPushed(string geotag, long userId, int pushId,  DailyPushState pushState)
        {
            await Task.Run(() => {

                var client = new RestClient($"{url}/v1/telegram/userByGeo");
                var request = new RestRequest(Method.POST);
                request.AddHeader($"Authorization", $"Bearer {token}");

                switch (pushState)
                {
                    case DailyPushState.sent:
                        tgUsersDailyPushSentDto sent = new();
                        sent.users.Add(new tgUserDailyPushSentDto() {
                            tg_geolocation = geotag,
                            tg_user_id = userId,
                            notification_send_id = pushId
                        });
                        string jsent = JsonConvert.SerializeObject(sent);
                        request.AddParameter("application/json", jsent, ParameterType.RequestBody);
                        break;
                    case DailyPushState.delivered:
                        tgUsersDailyPushDeliveredDto delivered = new();
                        delivered.users.Add(new tgUserDailyPushDeliveredDto() {
                            tg_geolocation = geotag,
                            tg_user_id = userId,
                            notification_delivered_id = pushId
                        });
                        string jdelivered = JsonConvert.SerializeObject(delivered);
                        request.AddParameter("application/json", jdelivered, ParameterType.RequestBody);
                        break;
                    case DailyPushState.disable:
                        tgUsersDailyPushDisableDto disable = new();
                        disable.users.Add(new tgUserDailyPushDisableDto() {
                            tg_geolocation = geotag,
                            tg_user_id = userId,
                            notification_enabled = false
                        });
                        string jdisable = JsonConvert.SerializeObject(disable);
                        request.AddParameter("application/json", jdisable, ParameterType.RequestBody);
                        break;
                    default:
                        throw new Exception("MarkFolloweWasDailyPushed Unknown DailyPushState");
                        
                }

                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = JObject.Parse(response.Content);
                    bool res = json["success"].ToObject<bool>();
                    if (!res)
                        throw new Exception($"MarkFollowerWasDailyPushed success={res}");

                }
                else
                    throw new Exception($"Не удалось пометить результат daily push подписчика geotag={geotag} userid={userId} pushid={pushId} pushstate={pushState} responce={response.Content}");
            });
        }

        class subAvaliableData
        {
            public bool is_available { get; set; }
            public int group_id { get; set; }
        }
        class subAvaliableResult
        {
            public bool success { get; set; }
            public subAvaliableData data { get; set; }
        }
        public virtual async Task<bool> IsSubscriptionAvaliable(string geotag, long id)
        {
            bool res = false;

            await Task.Run(() =>
            {
                var client = new RestClient($"{url}/v1/telegram/subscriptionAvailability?geo={geotag}&userID={id}");
                var request = new RestRequest(Method.GET);
                request.AddHeader($"Authorization", $"Bearer {token}");
                var response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resp = JsonConvert.DeserializeObject<subAvaliableResult>(response.Content);

                    if (resp.success)
                        res = resp.data.is_available;
                    else
                        throw new Exception($"IsSubscriptionAvaliable success={resp.success}");

                } else
                    throw new Exception($"Не удалось получить информацию о возможности подписки {id} на канал {geotag}");
            });

            return res;
        }
        #endregion
    }
}
