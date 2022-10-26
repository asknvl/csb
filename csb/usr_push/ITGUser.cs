using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace csb.usr_push
{
    public interface ITGUser
    {
        [JsonProperty]
        public string api_id { get; set; }
        [JsonProperty]
        public string api_hash { get; set; }        
        [JsonProperty]
        string geotag { get; set; }
        [JsonProperty]
        string phone_number { get; set; }
        Task Start();
        void Stop();
        void SetVerifyCode(string code);

        public event Action<string> VerificationCodeRequestEvent;
        public event Action<User> UserStartedResultEvent;
    }
}
