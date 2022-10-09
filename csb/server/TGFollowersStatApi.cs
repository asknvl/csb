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
        #endregion
    }
}
