using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public interface ITGUserManager<T>
    {
        IEnumerable<T> Users { get; }        
        void Add(T user);
        T Get(string phone);
        void Delete(string geotag);
        void Delete(T user);
        void StartAll();

        event Action<T> VerificationCodeRequestEvent;
    }

    public class UserPushManagerException : Exception
    {
        public UserPushManagerException(string message) : base(message) { }
    }
}
