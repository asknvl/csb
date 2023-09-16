using csb.telemetry;
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
        public bool IsRunning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BaseTelemetryProcessor Telemetry { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
            bool res = false;
            await Task.Run(() => {                
                VerificationCodeRequestEvent?.Invoke(geotag);
                verifyCodeReady.Reset();
                verifyCodeReady.Wait();
                res = true;

            }).ContinueWith(t =>
            {
                Console.WriteLine($"Введен код для {phone_number} = {verifyCode}");
                UserStartedResultEvent?.Invoke(geotag, res);
            });

            //Console.WriteLine($"Введен код для {phone_number} = {verifyCode}");
            //UserStartedResultEvent?.Invoke(usr);
        }

        public void Stop()
        {
            
        }

        public event Action<string> VerificationCodeRequestEvent;
        public event Action<string, bool> UserStartedResultEvent;
    }
}
