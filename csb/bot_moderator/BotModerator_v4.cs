using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using csb.messaging;
using Newtonsoft.Json;

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
            dailyPushTimer.Interval = 60 * 1000;
            dailyPushTimer.AutoReset = true;
            dailyPushTimer.Elapsed += DailyPushTimer_Elapsed; ;
            dailyPushTimer.Start();
        }

        int cntr = 0;
        private async void DailyPushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            try
            {
                string date_from = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string date_to = DateTime.Now.ToString("yyyy-MM-dd");
                var subs = await statApi.GetNoFeedbackFollowers(GeoTag, date_from, date_to);

                foreach (var subscriber in subs)
                {
                    long id = long.Parse(subscriber.tg_user_id);

                    //var message = DailyPushData.Messages.FirstOrDefault(m => m.Id >= 0);

                    var message = DailyPushData.Messages[cntr];
                    cntr++;
                    cntr %= DailyPushData.Messages.Count;

                    if (message != null)
                        await message.Send(id, bot);
                }

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
