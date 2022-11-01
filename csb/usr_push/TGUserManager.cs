using csb.storage;
using csb.users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public class TGUserManager<T> : ITGUserManager<T> where T : ITGUser
    {
        #region vars
        IStorage<IEnumerable<T>> storage;        
        #endregion

        #region properties
        [JsonProperty]
        public IEnumerable<T> Users { get; private set; } = new List<T>();
        #endregion

        public TGUserManager(string ownerid) {

            string subdir = Path.Combine("admins", ownerid);
            storage = new Storage<IEnumerable<T>>("admins.json", subdir, Users);
            Users = storage.load();
            foreach (var user in Users)
            {
                user.VerificationCodeRequestEvent += User_VerificationCodeRequestEvent;
                user.UserStartedResultEvent += User_UserStartedResultEvent;
            }
        }

        #region private
        private void User_UserStartedResultEvent(string geotag, bool res)
        {
            UserStartedResultEvent?.Invoke(geotag, res);
        }

        private void User_VerificationCodeRequestEvent(string geotag)
        {
            VerificationCodeRequestEvent?.Invoke(geotag);
        }
        #endregion

        #region public        
        public void Add(T user)
        {
            var found = Users.Any(u => u.phone_number.Equals(user.phone_number));
            if (!found)
            {
                Users = Users.Append(user);
                user.VerificationCodeRequestEvent += User_VerificationCodeRequestEvent;
                user.UserStartedResultEvent += User_UserStartedResultEvent;
                storage.save(Users);
            }
            else throw new UserPushManagerException($"Номер {user.phone_number} уже зарегестрирован в системе");
        }

        public void Delete(T user)
        {
            throw new NotImplementedException("Метод не реализован");
        }

        public void Delete(string geotag)
        {
            var user = Get(geotag);
            user?.Stop();

            var users = Users.ToList();
            users.RemoveAll(u => u.geotag.Equals(geotag));
            Users = users;
            storage.save(Users);
        }

        public T Get(string geotag)
        {
            return Users.FirstOrDefault(u => u.geotag.Equals(geotag));
        }

        public async void StartAll()
        {         
            foreach (var user in Users) 
                await user.Start();
        }

        #endregion

        #region events        
        public event Action<string> VerificationCodeRequestEvent;
        public event Action<string, bool> UserStartedResultEvent;
        #endregion

    }
}
