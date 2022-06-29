using csb.bot_poster;
using csb.usr_listener;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csb.chains
{
    public class Chain : IChain
    {
        #region vars
        UserListener user;
        BotPoster_api bot;
        #endregion

        #region properties
        public long Owner { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public string PhoneNumber { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public long OutputChannelID { get; set; }
        [JsonProperty]
        public string OutputChannelTitle { get; set; }        
        public bool IsRunning
        {
            get
            {
                if (bot == null || user == null)
                    return false;
                return bot.IsRunning & user.IsRunning;
            }
        }
        [JsonProperty]
        public string OutputBotName
        {
            get
            {
                if (bot == null)
                    return "?";
                else
                    return bot.OutputBotName;
            }
        }

        #endregion

        public Chain()
        {
        }

        public void Start()
        {
            user = new UserListener(PhoneNumber);
            bot = new BotPoster_api(Token);

            try
            {
                user.NeedVerifyCodeEvent += (phone) =>
                {
                    NeedVerifyCodeEvent?.Invoke(Id, phone);
                };

                bot.OutputChannelID = OutputChannelID;
                bot.Start();

                user.CorrespondingBotName = bot.OutputBotName;
                user.Start();


                //int ctr;
                //while (!(bot.IsRunning & user.IsRunning))
                //{
                //    Thread.Sleep(1000);
                //    Console.WriteLine($"starting chain Id={Id}...");                    
                //}

                Console.WriteLine($"chain Id={Id} started");

            } catch (Exception ex)
            {
            }
        }

        public void Stop()
        {
            bot?.Stop();
            user?.Stop();
        }

        public void SetVerifyCode(string code)
        {
            user.SetVerifyCode(code);
        }

        public event Action<int, string> NeedVerifyCodeEvent;

        public override string ToString()
        {
            return $"{Name} Id={Id}";
        }

    }
}
