using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using csb.messaging;
using Newtonsoft.Json;
using static csb.server.TGFollowersStatApi;

namespace csb.bot_moderator
{
    public class BotModerator_v4 : BotModerator_v3
    {
        #region vars
        System.Timers.Timer dailyPushTimer = new System.Timers.Timer();
        #endregion

        #region properties
        [JsonProperty]
        public DailyPushData DailyPushData { get; set; } = new();
        #endregion

        public BotModerator_v4(string token, string geotag) : base(token, geotag)
        {
            dailyPushTimer.Interval = 10 * 60 * 1000;
            dailyPushTimer.AutoReset = true;
            dailyPushTimer.Elapsed += DailyPushTimer_Elapsed;
            dailyPushTimer.Start();
        }

        int cntr = 0;
        private async void DailyPushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        { 
            try
            {
                if (DailyPushData.Messages.Count == 0)
                    return;

#if DEBUG
                var message = DailyPushData.Messages.FirstOrDefault(m => m.Id > cntr);

                if (message != null)
                {
                    Console.WriteLine($"{cntr}{message.Id}");
                    //if (cntr == 4)
                    //{
                    //    Console.WriteLine("set fileId=null");
                    //    message.fileId = null;
                    //}
                    await message.Send(1780912435, bot);
                    cntr++;
                }
                else
                    cntr = 0;
#else

                var subs = await statApi.GetUsersNeedDailyPush(GeoTag, 24);
                Console.WriteLine($"{DateTime.Now} GetSubs {GeoTag} {subs.Count}");

                foreach (var subscriber in subs)
                {
                    try
                    {
                        long id = long.Parse(subscriber.tg_user_id);
                        int pushId_prev = (subscriber.notification_delivered_id == null) ? 0 : (int)subscriber.notification_delivered_id;
                        var message = DailyPushData.Messages.FirstOrDefault(m => m.Id > pushId_prev);

                        if (message != null)
                        {
                            await statApi.MarkFollowerWasDailyPushed(GeoTag, id, message.Id, DailyPushState.sent);
                            try
                            {
                                await message.Send(id, bot);
                                await statApi.MarkFollowerWasDailyPushed(GeoTag, id, message.Id, DailyPushState.delivered);
                                Console.WriteLine($"{GeoTag} {id} was pushed {message.Id}");
                            }
                            catch (Exception ex)
                            {
                                await statApi.MarkFollowerWasDailyPushed(GeoTag, id, 0, DailyPushState.disable);
                                throw;
                            }
                        }
                        else
                        {
                            await statApi.MarkFollowerWasDailyPushed(GeoTag, id, 0, DailyPushState.disable);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{GeoTag} {subscriber.tg_user_id} {subscriber?.notification_delivered_id} {ex.Message}");
                    }
                }
#endif

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void Stop()
        {
            base.Stop();
            dailyPushTimer.Stop();
        }
    }
}
