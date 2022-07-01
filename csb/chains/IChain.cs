using csb.bot_poster;
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
        bool IsRunning { get; }

        public void Start();
        public void Stop();

        Task AddInputChannel(string input);

        void SetVerifyCode(string code);
    }
}
