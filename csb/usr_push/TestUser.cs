using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;

namespace csb.usr_push
{
    public class TestUser : ITGUser
    {
        #region properties
        public string geotag { get; set; }
        public string phone_number { get; set; }
        public string api_id { get; set; }
        public string api_hash { get; set; }
        #endregion

        #region vars
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;
        #endregion

        public TestUser(string number)
        {
            phone_number = number;
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }

        public async Task Start()
        {
            User usr = null;
            await Task.Run(() => {                
                VerifyCodeRequestEvent?.Invoke(this);
                verifyCodeReady.Reset();
                verifyCodeReady.Wait();

            }).ContinueWith(t =>
            {
                Console.WriteLine($"Введен код для {phone_number} = {verifyCode}");
                UserStartedResultEvent?.Invoke(usr);
            });

            //Console.WriteLine($"Введен код для {phone_number} = {verifyCode}");
            //UserStartedResultEvent?.Invoke(usr);
        }

        public event Action<ITGUser> VerifyCodeRequestEvent;
        public event Action<User> UserStartedResultEvent;
    }
}
