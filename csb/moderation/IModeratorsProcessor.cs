using csb.bot_moderator;
using csb.bot_poster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.moderation
{
    public interface IModeratorsProcessor
    {
        IEnumerable<IBotModerator> ModeratorBots { get; }
        void Add(string token, string geotag);
        void Add(string token, string geotag, DailyPushData patternPushData, List<AutoChange> autoChanges);
        GreetingsData Greetings(string geotag);
        PushData PushData(string geotag);
        DailyPushData DailyPushData(string geotag);
        IBotModerator Get(string geotag);
        void Delete(string geotag);
        void Start(string geotag);
        void Stop(string geotag);   
        void StartAll();

        void Load();
        void Save();
    }
}
