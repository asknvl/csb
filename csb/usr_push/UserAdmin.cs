﻿
using asknvl.logger;
using csb.server;
using csb.telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://185.46.9.229:4000");
#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");       
#endif

        CircularBuffer incomeIds = new CircularBuffer(50);
        CircularBuffer outcomeIds = new CircularBuffer(50);

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

                    logger.inf_urgent($"{geotag} AutoAnswerTimer elpased: ids={ids.Count}");

                    foreach ( var id in ids )
                    {
                        TL.User auto_msg_user = null;
                        bool found = _users.TryGetValue(id, out auto_msg_user);
                        if (found)
                        {
                            var peer = new InputPeerUser(auto_msg_user.ID, auto_msg_user.access_hash);
                            var history = await user.Messages_GetHistory(peer);

                            var alreadyReplied = history.Messages.Any(m => ((TL.Message)m).flags.HasFlag(TL.Message.Flags.out_));
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
                }
            }

            catch(Exception ex)
            {
                logger.err($"AutoAnswerTimer: {ex.Message}");
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
                                    await statApi.MarkFollowerWasReplied(geotag, id);                                    
                                } catch (Exception ex)
                                {
                                    r = false;
                                    err = ex.Message;

                                } finally
                                {
                                    logger.inf_urgent($"User id={id} fn={fn} ln={ln} un={un} REPLIED={r} ({err}) {geotag}");
                                    outcomeIds.Add(id);
                                    logger.inf_urgent(outcomeIds.ToString());
                                }
                            }
                        }
                        else
                        {
                            if (!incomeIds.ContainsID(id))
                            {
                                bool f = true;
                                try
                                {
                                    await statApi.MarkFollowerMadeFeedback(geotag, id);
                                } catch (Exception ex)
                                {
                                    f = false;
                                    err = ex.Message;
                                } finally
                                {
                                    logger.inf_urgent($"User id={id} fn={fn} ln={ln} un={un} FEEDBACK={f} ({err}) {geotag}");
                                    incomeIds.Add(id);
                                    logger.inf_urgent(incomeIds.ToString());
                                }                                
                            }
                        }

                        break;
                    case MessageService ms:
                        break;
                }
            } catch (Exception ex)
            {
                logger.err($"HandleMessage error: id={id} {ex.Message}");
            }
        }


        protected override async void processUpdates(UpdatesBase updates)
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

                //Random r = new Random();
                //r.
                //autoAnswerTimer.;

                //await Task.Delay(r.);

                //Task.Run(() => {
                //    Random r = new Random();
                //    r.Next(1, 10);
                //});

                autoAnswerTimer.Start();


            });
        }
        #endregion
    }
}
