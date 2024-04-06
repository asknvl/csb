using csb.bot_moderator;
using csb.bot_poster;
using csb.messaging;
using csb.moderation;
using csb.usr_listener;
using csb.usr_push;
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
        public DailyPushData DailyPushData { get; set; } = new();
        [JsonProperty]
        public SmartPushData PushData { get; set; } = new();
        [JsonProperty]
        public AutoAnswerData AutoAnswerData { get; set; } = new();

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
                User = new UserListener_v1(PhoneNumber, Name);
            }

            //User = new UserListener(PhoneNumber);
            User.PhoneNumber = PhoneNumber;
            User.Name = Name;
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

        public async Task AddBot(string token, string geotag)
        {
            var found = Bots.FirstOrDefault(t => t.Token.Equals(token));
            if (found == null)
            {
                var bot = new BotPoster_api(token);
                bot.GeoTag = geotag;

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

        public void EditBotGeotag(string name, string newgeotag)
        {
            var found = Bots.FirstOrDefault(t => t.Name.Equals(name));
            if (found != null)
            {
                found.GeoTag = newgeotag;
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

        public void AddAutoChange(string botname, AutoChange autochange)
        {
            var bot = Bots.FirstOrDefault(b => b.Name.Equals(botname));
            if (bot != null)
                bot.AutoChanges.Add(autochange);
        }

        public List<AutoChange> GetAutoChanges(string botname)
        {
            var bot = Bots.FirstOrDefault(b => b.Name.Equals(botname));
            if (bot != null)
                return bot.AutoChanges;
            else
                throw new Exception($"Для бота {bot.GeoTag} {bot.Name} не установлены автозамены");
        }

        public void RemoveAutoChange(string botname, int index)
        {
            var bot = Bots.FirstOrDefault(b => b.Name.Equals(botname));
            if (bot != null)
                bot.AutoChanges.RemoveAt(index);
        }

        public void ClearAutoChanges(string botname)
        {
            var bot = Bots.FirstOrDefault(b => b.Name.Equals(botname));
            if (bot != null)
                bot.AutoChanges.Clear();         
        }

        public void AddDailyPushMessage(DailyPushMessage pattern, IModeratorsProcessor moderators)
        {

            DailyPushData.Messages.Add(pattern);
            pattern.Id = DailyPushData.Messages.Count;
            
            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;
                
                AutoChange pmAutochange = new AutoChange()
                {
                    OldText = outbot.VictimLink,
                    NewText = outbot.ChannelLink
                };

                var patternCpy = pattern.Clone();
                //patternCpy.Id = DailyPushData.Messages.Count;

                patternCpy.MakeAutochange(new List<AutoChange>() { pmAutochange }); //pm
                patternCpy.MakeAutochange(outbot.AutoChanges); //bot poster autochanges

                try
                {
                    moderators.DailyPushData(geotag).Messages.Add(patternCpy);
                } catch (Exception ex)
                {

                }
                
            }
            moderators.Save();
            
        }

        public void ClearDailyPushMessages(IModeratorsProcessor moderators)
        {
            DailyPushData.Messages.Clear();
            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;
                try
                {
                    moderators.DailyPushData(geotag).Messages.Clear();
                } catch (Exception ex)
                {

                }
            }
            moderators.Save();
                
        }

        public void AddSmartPushMessage(SmartPushMessage pattern, IModeratorsProcessor moderators)
        {

            PushData.Messages.Add(pattern);
            pattern.Id = PushData.Messages.Count;

            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;

                AutoChange pmAutochange = new AutoChange()
                {
                    OldText = outbot.VictimLink,
                    NewText = outbot.ChannelLink
                };

                var patternCpy = pattern.Clone();
                //patternCpy.Id = DailyPushData.Messages.Count;

                patternCpy.MakeAutochange(new List<AutoChange>() { pmAutochange }); //pm
                patternCpy.MakeAutochange(outbot.AutoChanges); //bot poster autochanges

                try
                {
                    moderators.SmartPushData(geotag).Messages.Add(patternCpy);
                }
                catch (Exception ex)
                {

                }

            }
            moderators.Save();

        }

        public void ClearSmartPushMessages(IModeratorsProcessor moderators)
        {
            PushData.Messages.Clear();
            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;
                try
                {
                    moderators.SmartPushData(geotag).Messages.Clear();
                }
                catch (Exception ex)
                {

                }
            }            
            moderators.Save();
        }

        public void AddAutoAnswerMessage(AutoAnswerMessage pattern, ITGUserManager<UserAdmin> admins)
        {
            AutoAnswerData.Messages.Clear();

            AutoAnswerData.Messages.Add(pattern);
            pattern.Id = AutoAnswerData.Messages.Count;

            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;
                var patternCpy = pattern.Clone();
                
                try
                {
                    var admin = admins.Get(outbot.GeoTag);//moderators.SmartPushData(geotag).Messages.Add(patternCpy);
                    if (admin != null)
                    {
                        admin.AutoAnswerData.Messages.Add(patternCpy);
                    }
                }
                catch (Exception ex)
                {

                }

            }
            admins.Save();
        }

        public void ClearAutoAnswerMessage(ITGUserManager<UserAdmin> admins)
        {
            AutoAnswerData.Messages.Clear();
            foreach (var outbot in Bots)
            {
                string geotag = outbot.GeoTag;               

                try
                {
                    var admin = admins.Get(outbot.GeoTag);//moderators.SmartPushData(geotag).Messages.Add(patternCpy);
                    if (admin != null)
                    {
                        admin.AutoAnswerData.Messages.Clear();
                    }
                }
                catch (Exception ex)
                {

                }

            }
            admins.Save();
        }
    }
}
