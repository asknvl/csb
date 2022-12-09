using csb.messaging;
using csb.server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.bot_moderator
{
    public class BotModerator_v3 : BotModerator_v2
    {
        #region vars
        System.Timers.Timer pushTimer = new System.Timers.Timer();        
        #endregion

        #region properties
        [JsonProperty]
        public PushData PushData { get; set; } = new();
        #endregion

        public BotModerator_v3(string token, string geotag) : base(token, geotag)
        {
            pushTimer.Interval =  10 * 60 * 1000;
            pushTimer.AutoReset = true;
            pushTimer.Elapsed += PushTimer_Elapsed;
            pushTimer.Start();
        }

        #region private
        private async void PushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string date_from = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string date_to = DateTime.Now.ToString("yyyy-MM-dd");
                var subs = await statApi.GetNoFeedbackFollowers(GeoTag, date_from, date_to);

                foreach (var subscriber in subs)         
                {

                    double lastPushSendHours = (subscriber.push_send_hours != null) ? (double)subscriber.push_send_hours : 0;
                    double lastPushDeliveredHours = (subscriber.push_delivered_hours != null) ? (double)subscriber.push_delivered_hours : 0;
                    double lastPushHours = Math.Max(lastPushSendHours, lastPushDeliveredHours);

                    double Tc = subscriber.time_after_subscribe;
                    double Tp = lastPushHours;
                    double Tl = subscriber.time_diff_last_push_subscr;
                    
                    //Console.WriteLine($"Tp={Tp} Tc={Tc} Tl={Tl}, Tc-Tl+Tp={Tc - Tl + Tp}");

                    var pushmessage = PushData.Messages.FirstOrDefault(m => m.TimePeriod > Tp && m.TimePeriod < Tc - Tl + Tp);

                    if (pushmessage != null)
                    {

                        long id = long.Parse(subscriber.tg_user_id);

                        try
                        {
                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, false);

                            await bot.SendTextMessageAsync(
                                                 id,
                                                 text: pushmessage.TextMessage.Text,
                                                 replyMarkup: pushmessage.TextMessage.ReplyMarkup,
                                                 entities: pushmessage.TextMessage.Entities,
                                                 disableWebPagePreview: false,
                                                 cancellationToken: new CancellationToken());

                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, true);
                            //Console.WriteLine($"PUSH: user {subscriber.tg_user_id} pushed with {pushmessage.TimePeriod} hour message");

                        } catch (Exception ex)
                        {
                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, false);
                            //Console.WriteLine($"PUSH: user {subscriber.tg_user_id} NOT pushed with {pushmessage.TimePeriod} hour message");
                        }

                    } else
                    {
                        //Console.WriteLine($"PUSH: no push messages for {subscriber.tg_user_id}");
                    }
                }

            } catch (Exception ex)
            {
                Console.WriteLine($"------------------------ {ex.Message} --------------------------------");
            }

        }
        #endregion

        int appCntr = 0;
        int decCntr = 0;
        #region override
        protected override async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {

            if (update == null)
                return;

            if (update.ChatMember != null)
            {
                try
                {
                    var member = update.ChatMember;

                    long user_id = member.NewChatMember.User.Id;
                    long chat_id = update.ChatMember.Chat.Id;

                    string fn = member.NewChatMember.User.FirstName;
                    string ln = member.NewChatMember.User.LastName;
                    string un = member.NewChatMember.User.Username;

                    string link = member.InviteLink?.InviteLink;

                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = chat_id,
                        tg_user_id = user_id,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        invite_link = link,
                        tg_geolocation = GeoTag
                    };

                    switch (member.NewChatMember.Status)
                    {
                        case Telegram.Bot.Types.Enums.ChatMemberStatus.Member:
                            follower.is_subscribed = true;

                            if (member.InviteLink != null)
                            {
                                if (member.InviteLink.CreatesJoinRequest)
                                {
                                    followers.Add(follower);
                                    await statApi.UpdateFollowers(followers);
                                    Console.WriteLine("Updated DB+");

                                }
                            }
                            break;

                        case Telegram.Bot.Types.Enums.ChatMemberStatus.Left:

                            try
                            {
                                if (Greetings.ByeMessage != null)
                                    await bot.SendTextMessageAsync(
                                             member.From.Id,
                                             text: Greetings.ByeMessage.Text,
                                             replyMarkup: Greetings.ByeMessage.ReplyMarkup,
                                             entities: Greetings.ByeMessage.Entities,
                                             disableWebPagePreview: false,
                                             cancellationToken: cancellationToken);
                            } catch (Exception ex) { }

                            follower.is_subscribed = false;
                            followers.Add(follower);
                            await statApi.UpdateFollowers(followers);
                            Console.WriteLine("Updated DB-");
                            break;
                    }

                    Console.WriteLine(follower);

                } catch (Exception ex)
                {
                    Console.WriteLine($"------------------------ {DateTime.Now} {ex.Message} --------------------------------");
                }
            }

            if (update.ChatJoinRequest != null)
            {
                
                try
                {
                    var chatJoinRequest = update.ChatJoinRequest;

                    var user_geotags = await statApi.GetFollowerGeoTags(chatJoinRequest.From.Id);
                    List<string> chGeoPrefx = new();

                    string tags = "";
                    foreach (var item in user_geotags)
                    {
                        chGeoPrefx.Add(item.Substring(0, 4));
                        tags += $"{item} ";
                    }

                    bool addme = false;
                    try
                    {
                        addme = addMe.IsApproved(chatJoinRequest.From.Id);
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"IsApproved? {ex.Message}");
                    }

                    //string geoPrefx = GeoTag.Substring(0, 4);
                    //if (!chGeoPrefx.Any(n => n.Equals(geoPrefx)) || addme)
                    bool isAllowed = await statApi.IsSubscriptionAvailable(GeoTag, chatJoinRequest.From.Id);
                    if (isAllowed || addme)
                    {
                        try
                        {
                            if (Greetings.HelloMessage != null)
                                await bot.SendTextMessageAsync(
                                         chatJoinRequest.From.Id,
                                         text: Greetings.HelloMessage.Text,
                                         replyMarkup: Greetings.HelloMessage.ReplyMarkup,
                                         entities: Greetings.HelloMessage.Entities,
                                         disableWebPagePreview: true,
                                         cancellationToken: cancellationToken);
                        } catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                        Console.WriteLine($"{DateTime.Now} {GeoTag} cntr={++appCntr} APPROVED {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");

                    } else
                    {
                        Console.WriteLine($"{DateTime.Now} {GeoTag} cntr={++decCntr} DECLINED {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");
                        await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine($"------------------------ {DateTime.Now} {ex.Message} --------------------------------");
                }
            }
        }
        #endregion
    }
}
