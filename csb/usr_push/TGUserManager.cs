﻿using csb.storage;
using csb.users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public TGUserManager(string jsonfilename) {
            storage = new Storage<IEnumerable<T>>(jsonfilename, Users);
            Users = storage.load();
        }

        #region public        
        public void Add(T user)
        {
            var found = Users.Any(u => u.phone_number.Equals(user.phone_number));
            if (!found)
            {
                Users = Users.Append(user);
                storage.save(Users);
            }
            else throw new UserPushManagerException($"Номер {user.phone_number} уже зарегестрирован в системе");

        }

        public void Remove(T user)
        {
            var users = Users.ToList();
            users.Remove(user);
            storage.save(Users);
        } 

        public ITGUser Get(string phone)
        {
            return Users.FirstOrDefault(u => u.phone_number.Equals(phone));
        }

        public async void StartAll()
        {         
            foreach (var user in Users) 
                await user.Start();
        }
        #endregion

    }
}
