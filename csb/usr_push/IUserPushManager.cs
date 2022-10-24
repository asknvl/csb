using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public interface IUserPushManager
    {
        IEnumerable<ITGUser> Users { get; }
        void Add(ITGUser user);
        ITGUser Get(string phone);
        void StartAll();
    }
}
