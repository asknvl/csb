using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public class UserPushManager<T> : IUserPushManager where T : TestUser
    {
        public IEnumerable<ITGUser> Users { get; private set; } = new List<T>();

        public void Add(ITGUser user)
        {
            var found = Users.Any(u => u.phone_number.Equals(user.phone_number));
            if (!found)
                Users = Users.Append(user);
        }

        public ITGUser Get(string phone)
        {
            return Users.FirstOrDefault(u => u.phone_number.Equals(phone));
        }

        public void StartAll()
        {         
            foreach (var user in Users) 
                user.Start();
        }
    }
}
