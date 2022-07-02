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
        public UserListener User { get; private set; }

        public bool IsRunning
        {
            get
            {
                if (/*bot == null ||*/ User == null)
                    return false;
                return /*bot.IsRunning &*/ User.IsRunning;
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
            await User.AddInputChannel(input);
        }

        public void Start()
        {
            User = new UserListener(PhoneNumber);
            //bot = new BotPoster_api(Token);

            try
            {
                User.NeedVerifyCodeEvent += (phone) =>
                {
                    NeedVerifyCodeEvent?.Invoke(Id, phone);
                };
                                
                foreach (var item in Bots)
                {                    
                    item.Start();                    
                    User.CorrespondingBotNames.Add(item.Name);
                }


                //bot.OutputChannelID = OutputChannelID;
                //bot.OutputChannelLink = OutputChannelLink;
                //bot.Start();

                
                User.Start();

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
            User?.Stop();
            Console.WriteLine($"chain Id={Id} stopped");
        }

        public void SetVerifyCode(string code)
        {
            User.SetVerifyCode(code);
        }

        public void AddBot(string token)
        {
            var found = Bots.FirstOrDefault(t => t.Token.Equals(token));
            if (found == null)
                Bots.Add(new BotPoster_api(token));
            else
                throw new Exception("Бот с таким токеном уже существет, введите другой токен");
        }

        public event Action<int, string> NeedVerifyCodeEvent;

        public override string ToString()
        {
            return $"{Name} Id={Id}";
        }

    }
}
