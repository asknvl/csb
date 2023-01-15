using csb.bot_moderator;
using csb.bot_poster;
using csb.messaging;
using csb.moderation;
using csb.usr_listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace csb.chains
{
    public interface IChain
    {
        long Owner { get; set; }
        string Name { get; set; }
        int Id { get; set; }
        string PhoneNumber { get; set; }
        List<BotPoster_api> Bots { get; set; }
        UserListener_v1 User { get; }
        public List<string> ReplacedWords { get; set; }
        //public List<AutoChange> AutoChanges { get; set; }
        public SmartPushData PushData { get; set; }
        public DailyPushData DailyPushData { get; set; }
        bool IsRunning { get; }
        ChainState State { get; set; }

        public void Start();
        public void Stop();

        void SetVerifyCode(string code);

        Task AddBot(string token);
        void RemoveBot(string name);
        void EditBotGeotag(string name, string newgeotag);

        void AddFilteredWord(string text);
        void RemoveFilteredWord(int index);
        void ClearFilteredWords();

        void SetMessagingPeriod(double period);
        double GetMessagingPeriod();

        void AddReplacedWord(string word);
        void RemoveReplacedWord(int index);
        void ClearReplacedWords();

        void AddAutoChange(string botname, AutoChange autochange);
        List<AutoChange> GetAutoChanges(string botname);
        void RemoveAutoChange(string botname, int index);
        void ClearAutoChanges(string botname);
        void AddDailyPushMessage(DailyPushMessage message, IModeratorsProcessor moderators);        
        void ClearDailyPushMessages(IModeratorsProcessor moderators);
        void AddSmartPushMessage(SmartPushMessage message, IModeratorsProcessor moderators);
        void ClearSmartPushMessages(IModeratorsProcessor moderators);


    }


    public enum ChainState
    {
        X,
        creating,
        edditing        
    }
         
}
