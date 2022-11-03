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
        #endregion
    }
}
