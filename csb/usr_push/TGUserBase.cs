using asknvl.logger;
using csb.chains;
using csb.telemetry;
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
        Messages_Dialogs _dialogs;

        System.Timers.Timer startWatchDogTimer;        
        System.Timers.Timer updatesWatchDogTimer;

        protected TGUserTelemetryObject telemetryOject;
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

        [JsonIgnore]
        public BaseTelemetryProcessor Telemetry { get; set; }
        #endregion

        public TGUserBase(string api_id, string api_hash, string phone_number, string geotag)
        {
            this.api_id = api_id;
            this.api_hash = api_hash;
            this.phone_number = phone_number;
            this.geotag = geotag;

            Telemetry = new TGUserTelemetryProcessor(geotag);
            telemetryOject = (TGUserTelemetryObject)Telemetry.TelemetryObject;

                
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

        abstract protected Task processUpdates(UpdatesBase update);
        #endregion

        #region private
        private async Task OnUpdate(UpdatesBase updates)
        {

            try
            {
                
                Console.WriteLine($"-------------------------> {geotag}");
                telemetryOject.Updates.UpdatesCntr++;

                //long newUserId = 0;

                switch (updates)
                {
                    case UpdatesTooLong utl:
                        Console.WriteLine($"-------------------------> {geotag} UpdatesTooLong ");
                        logger.inf("UpdatesTooLong");
                        var state = await user.Updates_GetState();
                        var ust = await user.Updates_GetDifference(state.pts, state.date, state.qts);
                        return;                 
                }

                //long newUserId = 0;


                foreach (var update in updates.UpdateList)
                {


                    switch (update)
                    {
                        case UpdateNewChannelMessage ucm:
                            //Чтобы сообщения от каналов не записывались
                            break;


                        case UpdateNewMessage unm:

                            //switch (unm.message)
                            //{
                            //    case TL.Message m:
                            //        break;
                            //    case TL.MessageService ms:
                            //        break;
                            //}

                            try
                            {

                                bool isIncoming = true;

                                var m = unm.message as TL.Message;
                                if (m != null)
                                {
                                    isIncoming = !m.flags.HasFlag(TL.Message.Flags.out_);
                                }

                                if (isIncoming)
                                {

                                    var id = unm.message.Peer.ID;
                                    if (!_users.ContainsKey(id))
                                    {
                                        //newUserId = id;

                                        updates.CollectUsersChats(_users, _chats);

                                        if (updates is UpdateShortMessage p_usm/* && !_users.ContainsKey(usm.user_id)*/)
                                        {
                                            if (_users.ContainsKey(p_usm.user_id))
                                            {
                                                _users.Remove(p_usm.user_id);
                                            }
                                        (await user.Updates_GetDifference(p_usm.pts - p_usm.pts_count, p_usm.date, 0)).CollectUsersChats(_users, _chats);
                                        }



                                        string? fn = null;
                                        string? ln = null;
                                        string? un = null;

                                        if (_users.ContainsKey(id))
                                        {
                                            fn = _users[id].first_name;
                                            ln = _users[id].last_name;
                                            un = _users[id].username;
                                        }

                                        await processNewUser(id, fn, ln, un);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.err($"OnUpdate 0: {ex.Message}");
                            }
                            break;
                    }

                }

                //updates.CollectUsersChats(_users, _chats);

                //if (updates is UpdateShortMessage usm/* && !_users.ContainsKey(usm.user_id)*/)
                //{
                //    if (_users.ContainsKey(usm.user_id))
                //    {
                //        _users.Remove(usm.user_id);
                //    }
                //    (await user.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(_users, _chats);
                //}





                //else if (updates is UpdateShortChatMessage uscm && (!_users.ContainsKey(uscm.from_id) || !_chats.ContainsKey(uscm.chat_id)))
                //    (await user.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(_users, _chats);

                //if (newUserId != 0)
                //{
                //    var fn = _users[newUserId].first_name;
                //    var ln = _users[newUserId].last_name;
                //    var un = _users[newUserId].username;
                //    await processNewUser(newUserId, fn: fn, ln: ln, un: un);
                //}

                await processUpdates(updates);

            }
            catch (Exception ex)
            {
                string s = $"OnUpdate 1: {ex.Message}";
                logger.err(s);
                Telemetry.AddException(s);
            }

        }

        abstract protected Task processNewUser(long user_id, string? fn = null, string? ln = null, string? un = null);        

        private async Task User_OnOther(IObject arg)
        {
            await Task.Run(async () => { 
            
                if (arg is ReactorError err)
                {

                    string s = $"Admin {geotag} ReactorError: {err.Exception.Message}";
                    Telemetry.AddException($"ReactorError: {err.Exception.Message}");
                    logger.err(s);

                    IsRunning = false;
                    int cntr = 0;

                    while (cntr < 10)
                    {

                        string msg = $"Disposing admin {geotag} and trying to reconnect in 5 seconds {++cntr}...";

                        logger.err(msg);
                        Telemetry.AddException(msg);

                        user?.Dispose();
                        user = null;

                        await Task.Delay(5000);

                        try
                        {
                            user = new Client(Config);
                            user.OnUpdate += OnUpdate;
                            user.OnOther += User_OnOther;
                            await user.LoginUserIfNeeded();
                            IsRunning = true;
                            break;

                        }
                        catch (Exception ex)
                        {
                            msg = $"Admin {geotag} connection still failing: " + ex.Message;
                            logger.err(msg);
                            Telemetry.AddException(msg);
                        }

                    }

                    if (!IsRunning)
                    {
                        var msg = $"Unable to start admin {geotag}";
                        logger.err(msg);
                        Telemetry.AddException(msg);
                    }

                }

            });
        }


        private void StartWatchDogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            telemetryOject.Updates.UpdatesCntrPrev = telemetryOject.Updates.UpdatesCntr;            
        }
        #endregion

        #region public
        public virtual Task Start()
        {
            TL.User usr = null;
            IsRunning = false;

            startWatchDogTimer = new System.Timers.Timer();
            startWatchDogTimer.Interval = 5 * 60 * 1000;
            startWatchDogTimer.Elapsed += (s, e) => {
                telemetryOject.Startup.IsStartedOk = false;
            };
            telemetryOject.Startup.IsStartedOk = true;
            startWatchDogTimer.Start();

            updatesWatchDogTimer = new System.Timers.Timer();
            updatesWatchDogTimer.Interval = 60 * 60 * 1000;
            updatesWatchDogTimer.AutoReset = true;
            startWatchDogTimer.Elapsed += StartWatchDogTimer_Elapsed;
            startWatchDogTimer.Start();

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

                var dialogs = await user.Messages_GetDialogs(limit: 200);
                dialogs.CollectUsersChats(_users, _chats);
                //int counter = 0;

                //foreach (var u in _users)
                //{
                //    try
                //    {

                //        var h = await user.Messages_GetHistory(u.Value, limit: 25);
                //        bool user_processed = false;
                //        var chat = _users[u.Key];

                //        foreach (var m in h.Messages)
                //        {
                //            var msg = m as TL.Message;
                //            if (msg.flags.HasFlag(TL.Message.Flags.out_))
                //            {
                //                user_processed = true;
                //                //Console.WriteLine($"БЫЛ ОТВЕТ!!! {chat.first_name} {chat.last_name} {++counter}");
                //                break;
                //            }
                //        }

                //        if (!user_processed && chat.id != usr.ID && !chat.IsBot && chat.id != 777000)
                //        {
                //            logger.inf("offline chat ->");
                //            await processNewUser(chat.id, u.Value.first_name, u.Value.last_name, u.Value.username);
                //            //Console.WriteLine($"НЕ БЫЛО ОТВЕТA!!! {chat.first_name} {chat.last_name} {++counter}");
                //        }
                //    } catch (Exception ex)
                //    {
                //        logger.inf($"Admin {geotag} GetHistory: {ex.Message}");
                //    }
                //}


                logger.inf($"Admin {geotag} started");

            }).ContinueWith(t =>
            {
                startWatchDogTimer.Stop();
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
