using csb.bot_poster;
using csb.usr_listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        bool IsRunning { get; }
        ChainState State { get; set; }

        public void Start();
        public void Stop();

        void SetVerifyCode(string code);
        Task AddBot(string token);
        void RemoveBot(string name);
        void AddFilteredWord(string text);
        void RemoveFilteredWord(int index);
        void ClearFilteredWords();
        void SetMessagingPeriod(double period);
        double GetMessagingPeriod();
    }


    public enum ChainState
    {
        X,
        creating,
        edditing        
    }
         
}
