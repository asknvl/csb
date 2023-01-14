using csb.bot_moderator;
using csb.bot_poster;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;
using static System.Net.Mime.MediaTypeNames;

namespace csb.moderation
{
    public class ModeratorsProcessor<T> : IModeratorsProcessor where T : IBotModerator
    {
        #region vars
        string path;
        #endregion

        #region properties
        //List<bot_moderator_capi> moderatorBotsList = new();
        //public List<bot_moderator_capi> ModeratorBots => moderatorBotsList;

        public IEnumerable<IBotModerator> ModeratorBots { get; private set; } = new List<IBotModerator>();

        #endregion

        public ModeratorsProcessor(string userId)
        {
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "moderators", $"{userId}");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            this.path = Path.Combine(dirPath, "moderators.json");
        }

        #region public
        public void Load()
        {
            if (!File.Exists(path))
            {
                Save();
            }
            string rd = File.ReadAllText(path);

            ModeratorBots = JsonConvert.DeserializeObject<IEnumerable<BotModeratorBase>>(rd);

            foreach (var bot in ModeratorBots)
            {

                if (bot.LeadType == null)
                {
                    bot.LeadType = BotModeratorLeadType.NO;
                    Save();
                }                

                bot.ParametersUpdatedEvent += (p) => {
                    Save();
                };
            }
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(ModeratorBots, Formatting.Indented);

            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json);

            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить файл JSON");
            }
        }

        public void Add(string token, string geotag)
        {
            bool found = ModeratorBots.Any(o => o.Token.Equals(token) || o.GeoTag.Equals(geotag));
            if (found)
                throw new Exception("Бот-модератор с таким токеном или геотегом уже существует. Повторите ввод:");

            T mbot = (T)Activator.CreateInstance(typeof(T), token, geotag);

            //var mbot = new bot_moderator_capi(token, geotag);
            mbot.ParametersUpdatedEvent += (p) =>
            {
                Save();
            };

            mbot.Start();
            ModeratorBots = ModeratorBots.Append(mbot);
            Save();
        }

        public void Add(string token, string geotag, DailyPushData patternPushData, List<AutoChange> autoChanges)
        {
            bool found = ModeratorBots.Any(o => o.Token.Equals(token) || o.GeoTag.Equals(geotag));
            if (found)
                throw new Exception("Бот-модератор с таким токеном или геотегом уже существует. Повторите ввод:");

            //var mbot = new bot_moderator_capi(token, geotag);
            //mbot.ParametersUpdatedEvent += (p) =>
            //{
            //    Save();
            //};

            T mbot = (T)Activator.CreateInstance(typeof(T), token, geotag);
            mbot.ParametersUpdatedEvent += (p) =>
            {
                Save();
            };

            foreach (var pattern in patternPushData.Messages)
            {
                pattern.MakeAutochange(autoChanges);
                mbot.DailyPushData.Messages.Add(pattern.Clone());
            }

            mbot.Start();
            ModeratorBots = ModeratorBots.Append(mbot);
            Save();
        }

        public GreetingsData Greetings(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception($"Бота-модератора с геотегом {geotag} не существует");
            return found.Greetings;
        }

        public SmartPushData SmartPushData(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception($"Бота-модератора с геотегом {geotag} не существует");
            return found.PushData;
        }

        public DailyPushData DailyPushData(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception($"Бота модератора с геотегом {geotag} не существует");
            return found.DailyPushData;
        }

        public IBotModerator Get(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            return found;
        }

        public void Delete(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            found.Stop();

            var moderators = ModeratorBots.ToList();
            moderators.RemoveAll(m => m.GeoTag.Equals(geotag));
            ModeratorBots = moderators;

            Save();
        }

        public void Start(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");

            found.Start();
        }

        public void Stop(string geotag)
        {
            var found = ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");

            found.Stop();
        }

        public void StartAll()
        {
            if (ModeratorBots == null)
                return;

            foreach (var item in ModeratorBots)
                if (!item.IsRunning)
                    item.Start();
        }
        #endregion
    }
}
