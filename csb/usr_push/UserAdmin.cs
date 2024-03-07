
using asknvl.logger;
using csb.server;
using csb.telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TL;
using WTelegram;

namespace csb.usr_push
{
    public class UserAdmin : TGUserBase
    {
        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
#if DEBUG
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v3("http://185.46.9.229:4000");
#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v3("https://ru.flopasda.site");       
#endif

        //CircularBuffer incomeIds = new CircularBuffer(50);
        CircularBuffer outcomeIds = new CircularBuffer(1024);

        System.Timers.Timer autoAnswerTimer = new System.Timers.Timer();
        #endregion

        #region properties 
        [JsonProperty]
        public bool NeedAutoAnswer { get; set; }
        [JsonProperty]
        public AutoAnswerData AutoAnswerData { get; set; } = new();
        #endregion

        public UserAdmin(string api_id, string api_hash, string phone_number, string geotag) : base(api_id, api_hash, phone_number, geotag)
        {
            autoAnswerTimer.Interval =  1 * 60 * 1000;
            autoAnswerTimer.AutoReset = true;
            autoAnswerTimer.Elapsed += AutoAnswerTimer_Elapsed;
        }

        private async void AutoAnswerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (NeedAutoAnswer && AutoAnswerData.Messages.Count > 0)
                {
                    var ids = await statApi.GetUsersNeedAutoAnswer(geotag, 60, 1.5);

                    //var ids = new List<long> { 1481806946, 5093436686};
                    //logger.inf_urgent($"{geotag} AutoAnswerTimer elpased: ids={ids.Count}");

                    foreach ( var id in ids )
                    {
                        TL.User auto_msg_user = null;
                        bool found = _users.TryGetValue(id, out auto_msg_user);
                        if (found)
                        {
                            var peer = new InputPeerUser(auto_msg_user.ID, auto_msg_user.access_hash);
                            var history = await user.Messages_GetHistory(peer);

                            bool alreadyReplied = false;

                            if (history != null)
                                alreadyReplied = history.Messages/*.Where(m => m is TL.Message)*/.Any(m => ((TL.Message)m).flags.HasFlag(TL.Message.Flags.out_));

                            if (!alreadyReplied)
                            {
                                logger.inf_urgent($"AutoAnswerTimer Sent to: {auto_msg_user.id} {auto_msg_user.first_name} {auto_msg_user.last_name} {auto_msg_user.username}");
                                await user.SendMessageAsync(/*auto_msg_user*/peer, AutoAnswerData.Messages[0].Message.Text);
                                //await statApi.MarkFollowerWasReplied(geotag, id);                                
                            }
                            else
                                logger.inf($"AutoAnswerTimer already had chat with: {auto_msg_user.id} {auto_msg_user.first_name} {auto_msg_user.last_name} {auto_msg_user.username}");

                            await statApi.MarkFollowerWasAutoMessaged(geotag, id);

                        }
                    }
                } else
                    if (NeedAutoAnswer && AutoAnswerData.Messages.Count == 0)
                        Telemetry.AddException("Не установлен автоответ");

            }

            catch(Exception ex)
            {

                string s = $"AutoAnswerTimer: {ex.Message}";
                logger.err(s);
                Telemetry.AddException(s);

            }
        }

        #region protected
        async Task HandleMessage(MessageBase messageBase)
        {
            long id = 0;
            try
            {
                switch (messageBase)
                {
                    case TL.Message m:                        

                        id = m.Peer.ID;
                        //var u = _users[id];

                        string fn = "udefined";
                        string ln = "undefined";
                        string un = "undefined";

                        if (_users.ContainsKey(id))
                        {
                            fn = _users[id].first_name;
                            ln = _users[id].last_name;
                            un = _users[id].username;
                        }                        

                        //var i = await user.Users_GetFullUser(_users[id]);

                        string err = "";

                        if (m.flags.HasFlag(TL.Message.Flags.out_))
                        {
                            if (!outcomeIds.ContainsID(id))
                            {
                                bool r = true;
                                
                                try
                                {
#if RELEASE
                                    await statApi.MarkFollowerWasReplied(geotag, id);                                    
#endif
                                } catch (Exception ex)
                                {
                                    r = false;
                                    err = ex.Message;
                                } finally
                                {
                                    logger.inf_urgent($"REPLIED: {id} {fn} {ln} {un} {r} ({err})");
                                    outcomeIds.Add(id);
                                    logger.inf_urgent(outcomeIds.ToString());
                                }
                            }
                        }
                        //else
                        //{
                        //    if (!incomeIds.ContainsID(id))
                        //    {
                        //        bool f = true;
                        //        try
                        //        {
                        //            await statApi.MarkFollowerMadeFeedback(geotag, id);
                        //        } catch (Exception ex)
                        //        {
                        //            f = false;
                        //            err = ex.Message;                                    
                        //        } finally
                        //        {
                        //            logger.inf_urgent($"User id={id} fn={fn} ln={ln} un={un} FEEDBACK={f} ({err}) {geotag}");
                        //            incomeIds.Add(id);
                        //            logger.inf_urgent(incomeIds.ToString());
                        //        }                                
                        //    }
                        //}

                        break;
                    case MessageService ms:
                        break;
                }
            } catch (Exception ex)
            {
                string s = $"HandleMessage error: id={id} {ex.Message}";
                logger.err(s);
                Telemetry.AddException(s);
            }
        }


        protected override async Task processUpdates(UpdatesBase updates)
        {
            foreach (var update in updates.UpdateList)
            {
                switch (update)
                {
                    case UpdateNewMessage unm:
                        await HandleMessage(unm.message);
                        break;
                }
            }
        }
#endregion

        #region override
        public override Task Start()
        {
            return base.Start().ContinueWith(async t =>
            {
                autoAnswerTimer.Start();
            });
        }

        protected override async Task processNewUser(long user_id, string fn = null, string ln = null, string un = null)
        {
            try
            {
                logger.inf($"WROTE: {user_id} {fn} {ln} {un}");
                //Debug.WriteLine($"WROTE: {user_id} {fn} {ln} {un}");
                //await Task.Delay(200);

#if RELEASE
                await statApi.MarkFollowerMadeFeedback(geotag, user_id, fn, ln, un);   
#endif
            }
            catch (Exception ex)
            {                
                logger.err($"processNewUser {user_id}: {ex.Message}");
                Telemetry.AddException($"Не удалось записать данные о первом сообщении лида в базу {user_id} {fn} {ln} {un} ({ex.Message})");
            } 
        }
#endregion
    }
}
