﻿using csb.server;
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
            pushTimer.Interval = 5000;
            pushTimer.AutoReset = true;
            pushTimer.Elapsed += PushTimer_Elapsed;
            pushTimer.Start();
        }

        #region private
        private async void PushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //ChatId id = new ChatId(1669011167);

            //try
            //{

            //    await bot.SendTextMessageAsync(
            //        id,
            //        text: "Push message",
            //        cancellationToken: new CancellationToken());
            //} catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

        }
        #endregion

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
                    string tags = "";
                    foreach (var item in user_geotags)
                        tags += $"{item} ";

                    bool addme = false;
                    try
                    {
                        addme = addMe.IsApproved(chatJoinRequest.From.Id);
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"IsApproved? {ex.Message}");
                    }

                    if (user_geotags.Count == 0 || (user_geotags.Count == 1 && user_geotags[0].Length != GeoTag.Length) || addme)
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
                        Console.WriteLine($"{DateTime.Now} {GeoTag} APPROVED {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");

                    } else
                    {
                        Console.WriteLine($"{DateTime.Now} {GeoTag} DECLINED {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");
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
