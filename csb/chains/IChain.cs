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
        string Token { get; set; }
        string OutputBotName { get; }
        long OutputChannelID { get; set; }
        public string OutputChannelTitle { get; set; }
        bool IsRunning { get; }

        public void Start();
        public void Stop();

        void SetVerifyCode(string code);
    }
}
