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
        string phone_number { get; set; }
        Task Start();
        void SetVerifyCode(string code);

        public event Action<ITGUser> VerifyCodeRequestEvent;
        public event Action<User> UserStartedResultEvent;
    }
}
