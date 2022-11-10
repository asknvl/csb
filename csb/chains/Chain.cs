using csb.bot_poster;
using csb.messaging;
using csb.usr_listener;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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
        [JsonProperty]
        public UserListener_v1 User { get; private set; }
        [JsonProperty]
        public List<string> ReplacedWords { get; set; } = new();
        [JsonProperty]
        public List<AutoChange> AutoChanges { get; set; } = new();
        [JsonProperty]
        public List<DailyPushMessage> DailyPushMessages { get; set; } = new();

        [JsonIgnore]        
        public bool IsRunning
        {
            get
            {
                if (/*bot == null ||*/ User == null)
                    return false;
                return /*bot.IsRunning &*/ User.IsRunning;
            }
        }

        public ChainState State {
            get; set;
        }
        #endregion

        public Chain()
        {
            
        }

        public async void Start()
        {

            if (IsRunning)
            {
                UserStartedEvent?.Invoke(this);
                return;
            }

            if (User == null)
            {
                User = new UserListener_v1(PhoneNumber);
            }

            //User = new UserListener(PhoneNumber);
            User.PhoneNumber = PhoneNumber;
            User.NeedVerifyCodeEvent += (phone) =>
            {
                NeedVerifyCodeEvent?.Invoke(Id, phone);
            };

            User.StartedEvent -= User_StartedEvent;
            User.StartedEvent += User_StartedEvent;

            try
            {                   
                foreach (var item in Bots)
                {
                    item.ReplacedWords = new List<string>(ReplacedWords);
                    item.Start();
                    User.AddCorrespondingBot(item.Name);
                } 
                User.Start();
                Console.WriteLine($"chain Id={Id} started");

            } catch (Exception ex)
            {
            }
        }

        private void User_StartedEvent(string phone)
        {
            foreach (var item in Bots)
                item.AllowedID = User.ID;

            UserStartedEvent?.Invoke(this);
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

        public async Task AddBot(string token)
        {
            var found = Bots.FirstOrDefault(t => t.Token.Equals(token));
            if (found == null)
            {
                var bot = new BotPoster_api(token);
                if (User != null)
                {
                    bot.AllowedID = User.ID;
                    bot.ReplacedWords = new List<string>(ReplacedWords);
                }

                Bots.Add(bot);
                try
                {
                    bot.Start();
                    if (User != null)
                    {
                        User.AddCorrespondingBot(bot.Name);
                        await User.RestoreBots();
                    }
                } catch (Exception ex)
                {
                    throw new Exception("Не удалось запустить бота");
                }


            }
                
            else
                throw new Exception("Бот с таким токеном уже существет, введите другой токен");
        }

        public void RemoveBot(string name)
        {
            var found = Bots.FirstOrDefault(t => t.Name.Equals(name));
            if (found != null)
            {
                User.CorrespondingBotNames.Remove(found.Name);
                found.Stop();
                Bots.Remove(found);                
            }
        }

        public void AddFilteredWord(string text)
        {
            if (!User.FilteredWords.Contains(text))
                User.FilteredWords.Add(text);            
        }
        public void RemoveFilteredWord(int index)
        {
            User.FilteredWords.RemoveAt(index);
        }

        public void ClearFilteredWords()
        {
            User.FilteredWords.Clear();
        }

        public void SetMessagingPeriod(double period)
        {
            User.TimeInterval = period;
        }

        public double GetMessagingPeriod()
        {
            return User.TimeInterval;
        }

        public event Action<int, string> NeedVerifyCodeEvent;
        public event Action<IChain> UserStartedEvent;

        public override string ToString()
        {
            return $"{Name} Id={Id}";
        }

        public void AddReplacedWord(string word)
        {
            if (!ReplacedWords.Contains(word))
                ReplacedWords.Add(word);
            foreach (var item in Bots)
                item.ReplacedWords.Add(word);            
        }

        public void RemoveReplacedWord(int index)
        {
            ReplacedWords.RemoveAt(index);
            foreach (var item in Bots)
                item.ReplacedWords.RemoveAt(index);
        }

        public void ClearReplacedWords()
        {
            ReplacedWords.Clear();
            foreach (var item in Bots)
                item.ReplacedWords.Clear();
        }

        public void AddAutoChange(AutoChange autochange)
        {
            AutoChanges.Add(autochange);
            foreach (var bot in Bots)
            {
                if (!bot.AutoChanges.Contains(autochange))
                    bot.AutoChanges.Add(autochange);
            }
        }

        public List<AutoChange> GetAutoChanges()
        {
            return AutoChanges;
        }

        public void RemoveAutoChange(int index)
        {
            AutoChanges.RemoveAt(index);
            foreach (var bot in Bots)
            {
                bot.AutoChanges.RemoveAt(index);
            }
        }

        public void ClearAutoChanges()
        {
            AutoChanges.Clear();
            foreach (var bot in Bots)
            {
                bot.AutoChanges.Clear();                
            }
            
        }

        public void AddDailyPushMessage(DailyPushMessage message)
        {
            DailyPushMessages.Add(message);
        }

        public void RemoveDailyPushMessage(int index)
        {
            
        }

        public void ClearDailyPushMessages()
        {
            throw new NotImplementedException();
        }

        
    }
}
