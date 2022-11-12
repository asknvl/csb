using csb.bot_moderator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.moderation
{
    public class ModerationProcessor
    {
        #region vars
        string path;
        List<BotModerator_v4> moderatorBotsList = new();
        #endregion

        #region properties
        public List<BotModerator_v4> ModeratorBots => moderatorBotsList;
        #endregion

        public ModerationProcessor(string userId)
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
            moderatorBotsList = JsonConvert.DeserializeObject<List<BotModerator_v4>>(rd);
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(moderatorBotsList, Formatting.Indented);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json);

            } catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить файл JSON");
            }
        }

        public void Add(string token, string geotag)
        {
            bool found = moderatorBotsList.Any(o => o.Token.Equals(token) || o.GeoTag.Equals(geotag));
            if (found)
                throw new Exception("Бот-модератор с таким токеном или геотегом уже существует. Повторите ввод:");

            var mbot = new BotModerator_v4(token, geotag);
            mbot.Start();
            moderatorBotsList.Add(mbot);
            Save();            
        }
        
        public GreetingsData Greetings(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            return found.Greetings;
        }

        public PushData PushData(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            return found.PushData;
        }

        public DailyPushData DailyPushData(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception($"Бота модератора с геотегом {geotag} не существует");
            return found.DailyPushData;
        }

        public BotModerator Get(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            return found;
        }

        public void Delete(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");
            found.Stop();
            moderatorBotsList.Remove(found);
            Save();
        }

        public void Start(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");

            found.Start();
        }

        public void Stop(string geotag)
        {
            var found = moderatorBotsList.FirstOrDefault(o => o.GeoTag.Equals(geotag));
            if (found == null)
                throw new Exception("Бота-модератора с таким геотегом не существует");

            found.Stop();
        }

        public void StartAll()
        {
            if (moderatorBotsList == null)
                return;

            foreach (var item in moderatorBotsList)
                if (!item.IsRunning)
                    item.Start();
        }
        #endregion
    }
}
