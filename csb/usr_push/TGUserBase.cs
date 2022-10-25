using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace csb.usr_push
{
    public abstract class TGUserBase : ITGUser
    {
        #region vars
        Client user;
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;        
        #endregion

        #region properties
        [JsonProperty]
        public string api_id { get; set; }
        [JsonProperty]
        public string api_hash { get; set; }
        [JsonProperty]        
        public string phone_number { get; set; }
        [JsonProperty]
        public string geotag { get; set; }        
        #endregion

        public TGUserBase(string api_id, string api_hash, string phone_number, string geotag)
        {
            this.api_id = api_id;
            this.api_hash = api_hash;
            this.phone_number = phone_number;
            this.geotag = geotag;
        }

        #region protected
        protected string Config(string what)
        {

            string dir = Path.Combine(Directory.GetCurrentDirectory(), "userpool");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            switch (what)
            {
                case "api_id": return api_id;
                case "api_hash": return api_hash;
                case "session_pathname": return $"{dir}/{phone_number}.session";
                case "phone_number": return phone_number;
                case "verification_code":
                    VerifyCodeRequestEvent?.Invoke(this);
                    verifyCodeReady.Reset();
                    verifyCodeReady.Wait();
                    return verifyCode;
                case "first_name": return "Stevie";  
                case "last_name": return "Voughan";  
                case "password": return "5555";  
                default: return null;
            }
        }
        #endregion

        #region private
        private void User_Update(TL.IObject arg)
        {
            if (arg is not Updates { updates: var updates }) return;
            foreach (var update in updates)
            {

            }
        }
        #endregion

        #region public
        public Task Start()
        {
            User usr = null;
            return Task.Run(async () => {
                user = new Client(Config);
                usr = await user.LoginUserIfNeeded();                
                user.Update += User_Update;
            }).ContinueWith(t => {               
                    UserStartedResultEvent?.Invoke(usr);
            });
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }
        #endregion

        #region events
        public event Action<ITGUser> VerifyCodeRequestEvent;
        public event Action<User> UserStartedResultEvent;
        #endregion
    }
}
