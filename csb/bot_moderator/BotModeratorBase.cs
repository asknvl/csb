using asknvl.leads;
using asknvl.logger;
using csb.addme_service;
using csb.invitelinks;
using csb.server;
using csb.server.tg_dtos;
using csb.telemetry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_moderator
{
    public class BotModeratorBase : IBotModerator
    {
        #region const        
        const double push_period = 8 * 60 * 1000; // мс = 10 минут
#if DEBUG
        const double daily_push_period = 1 * 60 * 1000; // мс = 3 минуты
#else
        const double daily_push_period = 10 * 60 * 1000; // мс = 10 минут
#endif
        #endregion

        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
        asknvl.logger.ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;

        System.Timers.Timer pushTimer;

        System.Timers.Timer dailyPushTimer;
        bool isFirstDailyPushTimer = true;

#if DEBUG
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://185.46.9.229:4000");
#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");
#endif
        protected ITGFollowerTrackApi trackApi = new TGFollowerTrackApi("https://app.alopadsa.ru");

        protected AddMeService addMe = AddMeService.getInstance();

        protected ILeadsGenerator leadsGenerator;
        protected IInviteLinksProcessor linksProcessor;

        int allowedSubscribeRequestCounter = 0;
        int pushStartCounter = 0;
        #endregion

        #region properies
        [JsonProperty]
        public string GeoTag { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public long? ChannelID { get; set; } = null;

        LeadAlgorithmType? leadType = LeadAlgorithmType.CAPIv1;
        [JsonProperty]
        public LeadAlgorithmType? LeadType
        {
            get => leadType;
            set
            {
                leadType = value;
                ParametersUpdatedEvent?.Invoke(this);
                Stop();
                Start();
            }
        }

        GreetingsData greeteings = new();
        [JsonProperty]
        public GreetingsData Greetings
        {
            get => greeteings;
            set
            {
                greeteings = value;               
            }
        }
        [JsonProperty]
        public SmartPushData PushData { get; set; } = new();
        [JsonProperty]
        public DailyPushData DailyPushData { get; set; } = new();

        [JsonIgnore]
        public bool IsRunning { get; set; }

        [JsonProperty]
        public bool NeedTelemetry { get; set; }

        [JsonIgnore]
        public Dictionary<string, int> PseudoLeads { get; set; } = new();

        [JsonIgnore]
        public BaseTelemetryProcessor Telemetry { get; set; }         
        #endregion

        public BotModeratorBase(string token, string geotag)
        {

            Token = token;
            GeoTag = geotag;

            logger = new Logger("MDR", "moderators", GeoTag);
            Telemetry = new BotModeratorTelemetryProcessor(GeoTag);

            pushTimer = new System.Timers.Timer();
            pushTimer.Interval = push_period;
            pushTimer.AutoReset = true;
            pushTimer.Elapsed += PushTimer_Elapsed;            
            pushTimer.Start();

            Random r = new Random();
            int offset = r.Next(1, 10);

            dailyPushTimer = new System.Timers.Timer(offset * 60 * 1000);
            //dailyPushTimer.Interval = daily_push_period;
            dailyPushTimer.AutoReset = true;
            dailyPushTimer.Elapsed += DailyPushTimer_Elapsed;
            dailyPushTimer.Start();

        }

        #region private
        int tstcntr = 0;
        async void PushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (PushData.Messages.Count == 0)
                return;

            int delivered = 0;

            try
            {
#if !DEBUG
                string date_from = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string date_to = DateTime.Now.ToString("yyyy-MM-dd");

                logger.inf_urgent($"SMART:GeoTag={GeoTag}");
                var subs = await statApi.GetNoFeedbackFollowers(GeoTag, date_from, date_to);
                logger.inf_urgent($"SMART:GeoTag={GeoTag} subs count={subs.Count}");

                foreach (var subscriber in subs)
                {

                    double lastPushSendHours = (subscriber.push_send_hours != null) ? (double)subscriber.push_send_hours : 0;
                    double lastPushDeliveredHours = (subscriber.push_delivered_hours != null) ? (double)subscriber.push_delivered_hours : 0;
                    double lastPushHours = Math.Max(lastPushSendHours, lastPushDeliveredHours);

                    double Tc = subscriber.time_after_subscribe;
                    double Tp = lastPushHours;
                    double Tl = subscriber.time_diff_last_push_subscr;


                    //if (subscriber.tg_user_id.Equals(2018370443))
                    //{
                    //    logger.inf_urgent("!!! 2018370443");

                    //    logger.inf_urgent($"lastPushSendHours={lastPushSendHours}");
                    //    logger.inf_urgent($"lastPushDeliveredHours={lastPushDeliveredHours}");
                    //    logger.inf_urgent($"lastPushHours={lastPushHours}");

                    //    logger.inf_urgent($"Tc={subscriber.time_after_subscribe}");
                    //    logger.inf_urgent($"Tp={lastPushHours}");
                    //    logger.inf_urgent($"Tl={subscriber.time_diff_last_push_subscr}");
                    //}


                    var pushmessage = PushData.Messages.FirstOrDefault(m => m.TimePeriod > Tp && m.TimePeriod < Tc - Tl + Tp);                    

                    if (pushmessage != null)
                    {
                        long id = long.Parse(subscriber.tg_user_id);

                        try
                        {
                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, false);
                            await pushmessage.Send(id, bot);
                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, true);
                            delivered++;
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, false);
                            } catch 
                            {
                            }

                            logger.inf_urgent($"SMART: user {subscriber.tg_user_id} NOT pushed with {pushmessage.TimePeriod} hour message {ex.Message}");
                            //throw;
                            //logger.err($"PUSH: user {subscriber.tg_user_id} NOT pushed with {pushmessage.TimePeriod} hour message {ex.Message}");
                        }

                    }
                    else
                    {
                        //No messages for user yet
                        //logger.inf_urgent("SMART:pusmessage=null");
                    }
                }
#else
                var msg = PushData.Messages.FirstOrDefault(m => m.Id > tstcntr);
                if (msg != null)
                {
                    tstcntr = msg.Id;
                    await msg.Send(1784884123, bot);
                }
#endif

            }
            catch (Exception ex)
            {
                logger.err(ex.Message);
            } finally
            {
                logger.inf_urgent($"{GeoTag} SMART_PUSH: delivered {delivered}");
            }
        }

        int cntr = 0;
        async void DailyPushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            if (isFirstDailyPushTimer)
            {
                isFirstDailyPushTimer = false;
                dailyPushTimer.Interval = daily_push_period;
            }

            int sent = 0;
            int delivered = 0;

            var p = ((BotModeratorTelemetryProcessor)Telemetry);
            var o = (BotModeratorTelemetryObject)p.TelemetryObject;

            try
            {
                if (DailyPushData.Messages.Count == 0)
                    return;

#if DEBUG
                //var message = DailyPushData.Messages.FirstOrDefault(m => m.Id > cntr);

                //if (message != null)
                //{
                //    Console.WriteLine($"{cntr}{message.Id}");
                //    //if (cntr == 4)
                //    //{
                //    //    Console.WriteLine("set fileId=null");
                //    //    message.fileId = null;
                //    //}
                //    await message.Send(1447725495, bot);
                //    cntr++;
                //}
                //else
                //    cntr = 0;
#else

                var subs = await statApi.GetUsersNeedDailyPush(GeoTag, 24);
                sent = subs.Count;

                //logger.inf($"{DateTime.Now} GetDailySubs {GeoTag} {subs.Count}");

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
                                delivered++;

                                //logger.inf($"{GeoTag} {id} was pushed {message.Id}");
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
                        logger.err($"{GeoTag} {subscriber.tg_user_id} {subscriber?.notification_delivered_id} {ex.Message}");
                    }
                }
#endif

            }
            catch (Exception ex)
            {
                logger.err(ex.Message);
            } finally
            {
                if (sent > 0)
                    logger.inf_urgent($"{GeoTag} DAILY_PUSH: delivered {delivered} of {sent}");
                o.DailyPushes.Sent += sent;
                o.DailyPushes.Delivered += delivered;
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var p = ((BotModeratorTelemetryProcessor)Telemetry);

            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{GeoTag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            p.AddException($"Telegram API Error");

            logger.err(ErrorMessage);            
            return Task.CompletedTask;
        }
        #endregion

        #region protected
        protected virtual void processMyChatMember(Update update)
        {
            if (ChannelID != null)
                return;

            long id = update.MyChatMember.Chat.Id;

            switch (update.MyChatMember.NewChatMember.Status)
            {
                case ChatMemberStatus.Administrator:
                case ChatMemberStatus.Member:
                    ChannelID = id;
                    linksProcessor.UpdateChannelID(ChannelID);
                    ParametersUpdatedEvent?.Invoke(this);
                    logger.inf($"Moderator added to channel/group ID={id}");
                    //int linksNumber = await linksProcessor.Generate(ChannelID, 10);
                    break;
                default:
                    break;
            }
        }

        protected virtual async Task processChatJoinRequest(Update update, CancellationToken cancellationToken)
        {
            var p = ((BotModeratorTelemetryProcessor)Telemetry);
            var o = (BotModeratorTelemetryObject)p.TelemetryObject;

            if (update.ChatJoinRequest != null)
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
                }
                catch (Exception ex)
                {
                    logger.err(ex.Message);
                }

#if DEBUG
                bool isAllowed = true;
#else
                bool isAllowed = await statApi.IsSubscriptionAvailable(GeoTag, chatJoinRequest.From.Id);
#endif
                if (isAllowed || addme)
                {
                    try
                    {
                        allowedSubscribeRequestCounter++;

                        if (!string.IsNullOrEmpty(Greetings.PushStartText) && Greetings.UsePushStartButton)
                        {
                            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                                new KeyboardButton[] { $"{Greetings.PushStartText}" },
                            })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = true,                                 
                            };

                            if (Greetings.HelloMessage?.Message != null)
                            {

                                if (Greetings.PreJoinMessage?.Message != null)
                                {
                                    try
                                    {
                                        await Greetings.PreJoinMessage.Send(chatJoinRequest.From.Id, bot);
                                    }
                                    catch (Exception ex) { }
                                }
                                //else
                                //    p.AddException("Не установлен кружок");


                                await Greetings.HelloMessage.Send(chatJoinRequest.From.Id, bot, replyKeyboardMarkup);
                                
                            }
                            else
                                p.AddException("Не установлено приветственное сообщение");
                        }
                        else
                        {
                            p.AddException("Не установленa старт-кнопка");

                            if (Greetings.HelloMessage?.Message != null)
                            {
                                if (Greetings.PreJoinMessage?.Message != null)
                                {
                                    try
                                    {
                                        await Greetings.PreJoinMessage.Send(chatJoinRequest.From.Id, bot);
                                    } catch (Exception ex) { }
                                }
                                //else
                                //    p.AddException("Не установлен кружок");

                                await Greetings.HelloMessage.Send(chatJoinRequest.From.Id, bot);
                            } else
                                p.AddException("Не установлено приветственное сообщение");

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.err(ex.Message);
                    }
                    await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.inf_urgent($"{GeoTag} APPROVED({++appCntr}) {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");

                    o.Subscribers.Approved++;

                    

                }
                else
                {
                    logger.inf_urgent($"{GeoTag} DECLINED({++decCntr}) {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");                    
                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);

                    o.Subscribers.Declined++;

                    await statApi.MarkFollowerWasDeclined(GeoTag, chatJoinRequest.From.Id);

                    switch (LeadType)
                    {
                        case LeadAlgorithmType.CAPIv1:
                        case LeadAlgorithmType.CAPIv2:

                            try
                            {
                                var lead_data = await trackApi.GetLeadData(chatJoinRequest.InviteLink.InviteLink);
                                string pixel = lead_data.fb_pixel;

                                if (PseudoLeads.ContainsKey(pixel))
                                {

                                    logger.inf_urgent($"PseudoLead: {GeoTag} {pixel} {PseudoLeads[pixel]}");

                                    if (PseudoLeads[pixel] > 0)
                                    {
                                        PseudoLeads[pixel]--;
                                        logger.inf_urgent($"PseudoLeads left: {PseudoLeads[pixel]}");

                                        await leadsGenerator.MakeFbOptimizationEvent(chatJoinRequest.InviteLink.InviteLink, chatJoinRequest.From.FirstName, chatJoinRequest.From.LastName);
                                        await linksProcessor.Revoke(ChannelID, chatJoinRequest.InviteLink.InviteLink);

                                    }
                                    else
                                    {
                                        //logger.inf_urgent($"No pseudoleads for pixel {pixel} left. Pixel removed");
                                        //PseudoLeads.Remove(pixel);
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                logger.err("mrk1 " + ex.Message);                                
                            }

                            break;
                    }



                    //switch (LeadType)
                    //{
                    //    case LeadAlgorithmType.CAPIv1:
                    //    case LeadAlgorithmType.CAPIv2:
                    //        if (PseudoLeads > 0)
                    //        {
                    //            logger.inf_urgent($"{GeoTag} {PseudoLeads}:");                                
                    //            await leadsGenerator.MakeFBLead(chatJoinRequest.InviteLink.InviteLink, chatJoinRequest.From.FirstName, chatJoinRequest.From.LastName);
                    //            await linksProcessor.Revoke(ChannelID, chatJoinRequest.InviteLink.InviteLink);
                    //            PseudoLeads--;
                    //        }
                    //        break;
                    //}
                }
            }
        }

        protected virtual async Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            var p = ((BotModeratorTelemetryProcessor)Telemetry);
            var o = (BotModeratorTelemetryObject)p.TelemetryObject;

            if (update.ChatMember != null)
            {

                var member = update.ChatMember;

                long user_id = member.NewChatMember.User.Id;
                long chat_id = update.ChatMember.Chat.Id;

                if (ChannelID == null)
                {
                    ChannelID = chat_id;
                    ParametersUpdatedEvent?.Invoke(this);
                }

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
                    office_id = (int)globals.office,
                    tg_geolocation = GeoTag
                };

                switch (member.NewChatMember.Status)
                {
                    case ChatMemberStatus.Member:

                        follower.is_subscribed = true;

                        if (member.InviteLink != null)
                        {
                            if (member.InviteLink.CreatesJoinRequest)
                            {
                                followers.Add(follower);
                                await statApi.UpdateFollowers(followers);

                                string linkToRevoke = string.Empty;
                                try
                                {
                                    linkToRevoke = await leadsGenerator.MakeFbOptimizationEvent(member.InviteLink.InviteLink,
                                                                                       firstname: follower.firstname,
                                                                                       lastname: follower.lastname); //return invite link to revoke
                                } catch (Exception ex)
                                {
                                    p.AddException($"Не удалось отправить лид в фб {ex.Message}");
                                }

                                try
                                {
                                    await linksProcessor.Revoke(ChannelID, link);
                                } catch (Exception ex)
                                {

                                }

                                try
                                {
                                    var nextLink = await linksProcessor.Generate(ChannelID);
                                } catch (Exception ex) { 
                                }
                            }
                        }
                        break;

                    case ChatMemberStatus.Left:

                        try
                        {
                            if (Greetings.ByeMessage.Text != null)
                            {
                                await bot.SendTextMessageAsync(
                                         member.From.Id,
                                         text: Greetings.ByeMessage.Text,
                                         replyMarkup: Greetings.ByeMessage.ReplyMarkup,
                                         entities: Greetings.ByeMessage.Entities,
                                         disableWebPagePreview: false,
                                         cancellationToken: cancellationToken);
                            }
                            else
                                p.AddException("Не установлено прощальное сообщение");
                        }
                        catch (Exception ex)
                        {                            
                            logger.err(ex.Message);                            
                        }

                        follower.is_subscribed = false;
                        followers.Add(follower);
                        o.Subscribers.Unsubscribed++;
                        await statApi.UpdateFollowers(followers);

                        break;
                }

                logger.inf(follower.ToString());

            }
        }

        int appCntr = 0;
        int decCntr = 0;
        protected virtual async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            var p = ((BotModeratorTelemetryProcessor)Telemetry);
            var o = (BotModeratorTelemetryObject)p.TelemetryObject;

            switch (update.Type)
            {
                case UpdateType.MyChatMember:
                    try
                    {
                        processMyChatMember(update);
                    }
                    catch (Exception ex)
                    {
                        logger.err(ex.Message);
                    }
                    break;

                case UpdateType.ChatJoinRequest:
                    try
                    {
                        await processChatJoinRequest(update, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.err(ex.Message);
                    }
                    break;

                case UpdateType.ChatMember:
                    try
                    {
                        await processChatMember(update, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.err(ex.Message);
                    }
                    break;

                case UpdateType.Message:
                    if (update.Message != null)
                    {
                        if (update.Message.Text != null)
                        {

                            string msg = update.Message.Text;

                            var pushStartMsg = (string.IsNullOrEmpty(Greetings.PushStartText)) ? "" : Greetings.PushStartText.Replace(Greetings.PushStartEmoji, "");

                            if (msg.Contains(pushStartMsg) && Greetings.HelloMessageReply != null)
                            {
                                try
                                {
                                    await bot.SendTextMessageAsync(
                                        update.Message.From.Id,
                                        text: Greetings.HelloMessageReply.Text,
                                        replyMarkup: Greetings.HelloMessageReply.ReplyMarkup,
                                        entities: Greetings.HelloMessageReply.Entities,
                                        disableWebPagePreview: false,
                                        cancellationToken: cancellationToken);

                                    logger.inf_urgent($"{GeoTag} pushStartCounter={++pushStartCounter} allowedSubscribeRequestCounter={allowedSubscribeRequestCounter} {update.Message.From.FirstName}  {update.Message.From.LastName} {update.Message.From.Id}");

                                    o.PushStart.Shown = allowedSubscribeRequestCounter;
                                    o.PushStart.Clicked = pushStartCounter;                                    

                                } catch (Exception ex)
                                {
                                    p.AddException("Не удалось отправить ответ на push-старт");
                                }
                            }

                        }
                    }
                    break;
            }

        }
        #endregion

        #region helpers
        InlineKeyboardMarkup ReplyKeyboardToInlineKeyboard(ReplyKeyboardMarkup replyKeyboard)
        {
            List<List<InlineKeyboardButton>> inlineKeyboardRows = new List<List<InlineKeyboardButton>>();

            foreach (var row in replyKeyboard.Keyboard)
            {
                List<InlineKeyboardButton> inlineRow = new List<InlineKeyboardButton>();

                foreach (var button in row)
                {
                    // You can customize the callback data as needed
                    InlineKeyboardButton inlineButton = InlineKeyboardButton.WithCallbackData(button.Text, button.Text);
                    inlineRow.Add(inlineButton);
                }

                inlineKeyboardRows.Add(inlineRow);
            }

            return new InlineKeyboardMarkup(inlineKeyboardRows);
        }
       
        #endregion

        #region public
        public virtual void Start()
        {
            logger.inf($"Startting moderator...");

            if (IsRunning)
                return;

            bot = new TelegramBotClient(Token);
            var u = bot.GetMeAsync().Result;
            Name = u.Username;

            cts = new CancellationTokenSource();

            linksProcessor = InviteLinkProcessorFactory.Create(GeoTag, LeadType, bot, trackApi);

            linksProcessor.ExceptionEvent += (s) => {
                var p = ((BotModeratorTelemetryProcessor)Telemetry);
                p.AddException(s);

            };

            leadsGenerator = LeadsGeneratorFactory.Create(GeoTag, LeadType, trackApi);

            //if (ChannelID != null)
            //    linksProcessor.Generate(ChannelID, 20).Wait();

            linksProcessor.StartLinkNumberControl(ChannelID, cts);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.ChatJoinRequest, UpdateType.ChatMember, UpdateType.MyChatMember, UpdateType.Message }
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            pushTimer?.Start();
            dailyPushTimer?.Start();

            IsRunning = true;

            var schid = (ChannelID != null) ? $"{ChannelID}" : "null";
            logger.inf($"Moderator started, bot username={Name} channelID={schid}");

        }

        public void Stop()
        {
            cts?.Cancel();
            IsRunning = false;

            pushTimer?.Stop();
            dailyPushTimer?.Stop();

            logger.inf($"Moderator stopped");
        }
        #endregion

        #region events
        public event Action<IBotModerator> ParametersUpdatedEvent;
        #endregion
    }
}
