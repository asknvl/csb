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
        public List<BotPoster_api> Bots { get; set; } = new();

        

        public bool IsRunning
        {
            get
            {
                if (/*bot == null ||*/ user == null)
                    return false;
                return /*bot.IsRunning &*/ user.IsRunning;
            }
        }


        //[JsonProperty]
        //public string OutputBotName
        //{
        //    get
        //    {
        //        if (bot == null)
        //            return "?";
        //        else
        //            return bot.OutputBotName;
        //    }
        //}

        //public string OutputChannelLink { get; set; }

        #endregion

        public Chain()
        {
        }

        public async Task AddInputChannel(string input)
        {
            await user.AddInputChannel(input);
        }

        public void Start()
        {
            user = new UserListener(PhoneNumber);
            //bot = new BotPoster_api(Token);

            try
            {
                user.NeedVerifyCodeEvent += (phone) =>
                {
                    NeedVerifyCodeEvent?.Invoke(Id, phone);
                };
                                
                foreach (var item in Bots)
                {                    
                    item.Start();                    
                    user.CorrespondingBotNames.Add(item.Name);
                }


                //bot.OutputChannelID = OutputChannelID;
                //bot.OutputChannelLink = OutputChannelLink;
                //bot.Start();

                
                user.Start();

                Console.WriteLine($"chain Id={Id} started");

            } catch (Exception ex)
            {
            }
        }

        public void Stop()
        {
            if (Bots != null)
            {
                foreach (var item in Bots)
                    item.Stop();
            }
            //bot?.Stop();
            user?.Stop();
            Console.WriteLine($"chain Id={Id} stopped");
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
