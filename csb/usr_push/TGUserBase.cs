﻿using asknvl.logger;
using csb.chains;
using csb.users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TL;
using WTelegram;

namespace csb.usr_push
{
    public abstract class TGUserBase : ITGUser
    {
        #region vars
        protected Client user;
        readonly ManualResetEventSlim verifyCodeReady = new();
        string verifyCode;
        protected ILogger logger;
        //protected readonly Dictionary<long, TL.User> Users = new();
        //protected readonly Dictionary<long, ChatBase> Chats = new();

        protected Dictionary<long, TL.User> _users = new();
        protected Dictionary<long, ChatBase> _chats = new();
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
        [JsonProperty]
        public string? username { get; set; }

        [JsonIgnore]
        public bool IsRunning { get; set; }
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
                    logger.inf($"Admin verification code request {geotag}...");
                    VerificationCodeRequestEvent?.Invoke(geotag);
                    verifyCodeReady.Reset();
                    verifyCodeReady.Wait();
                    logger.inf($"Admin verification code obtained");
                    return verifyCode;
                case "first_name": return "Stevie";  
                case "last_name": return "Voughan";  
                case "password": return "5555";  
                default: return null;
            }
        }

        abstract protected void processUpdates(UpdatesBase update);
        #endregion

        #region private
        private async Task OnUpdate(UpdatesBase updates)
        {            
            updates.CollectUsersChats(_users, _chats);

            if (updates is UpdateShortMessage usm && !_users.ContainsKey(usm.user_id))
                (await user.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(_users, _chats);
            else if (updates is UpdateShortChatMessage uscm && (!_users.ContainsKey(uscm.from_id) || !_chats.ContainsKey(uscm.chat_id)))
                (await user.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(_users, _chats);

            await Task.Run(async () =>
            {
                processUpdates(updates);
            });
        }

        private async Task User_OnOther(IObject arg)
        {
            await Task.Run(() => { 
            
                if (arg is ReactorError err)
                {
                    logger.err($"Admin {geotag} ReactorError: {err.Exception.Message}");
                }

            });
        }

        #endregion

        #region public
        public virtual Task Start()
        {
            TL.User usr = null;
            IsRunning = false;

            logger = new Logger("ADM", "admins", $"{geotag}_{phone_number}");

            return Task.Run(async () =>
            {
                logger.inf($"Starting admin {geotag}...");
                user = new Client(Config);
                usr = await user.LoginUserIfNeeded();
                username = usr.username;
                user.OnUpdate -= OnUpdate;
                user.OnUpdate += OnUpdate;

                user.OnOther -= User_OnOther;
                user.OnOther += User_OnOther;

                IsRunning = true;



                //var dialogs = await user.Messages_GetAllDialogs();
                //dialogs.CollectUsersChats(_users, _chats);

                logger.inf($"Admin {geotag} started");

            }).ContinueWith(t =>
            {
                UserStartedResultEvent?.Invoke(geotag, IsRunning);
            });
        }

        public void SetVerifyCode(string code)
        {
            verifyCode = code;
            verifyCodeReady.Set();
        }

        public void Stop()
        {            
            user?.Dispose();
            IsRunning = false;
        }
        
        #endregion

        #region events
        public event Action<string> VerificationCodeRequestEvent;
        public event Action<string, bool> UserStartedResultEvent;
        #endregion

     
    }
}
