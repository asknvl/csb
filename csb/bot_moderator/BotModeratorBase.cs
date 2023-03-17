using asknvl.leads;
using asknvl.logger;
using csb.addme_service;
using csb.invitelinks;
using csb.server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace csb.bot_moderator
{
    public class BotModeratorBase : IBotModerator
    {
        #region const        
        const double push_period = 8 * 60 * 1000; // мс = 10 минут
#if DEBUG
        const double daily_push_period = 8 * 60 * 1000; // мс = 3 минуты
#else
        const double daily_push_period = 10 * 60 * 1000; // мс = 10 минут
#endif
        #endregion

        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
        ILogger logger;
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;

        System.Timers.Timer pushTimer = new System.Timers.Timer();
        System.Timers.Timer dailyPushTimer = new System.Timers.Timer();

#if DEBUG
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://185.46.9.229:4000");
#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");
#endif
        protected ITGFollowerTrackApi trackApi = new TGFollowerTrackApi("https://app.alopadsa.ru");

        protected AddMeService addMe = AddMeService.getInstance();

        protected ILeadsGenerator leadsGenerator;
        protected IInviteLinksProcessor linksProcessor;
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

        LeadAlgorithmType? leadType = null;
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
        [JsonProperty]
        public GreetingsData Greetings { get; set; } = new();
        [JsonProperty]
        public SmartPushData PushData { get; set; } = new();
        [JsonProperty]
        public DailyPushData DailyPushData { get; set; } = new();

        [JsonIgnore]
        public bool IsRunning { get; set; }

        #endregion

        public BotModeratorBase(string token, string geotag)
        {

            Token = token;
            GeoTag = geotag;

            logger = new Logger("MDR", "moderators", GeoTag);

#if !DEBUG
            pushTimer.Interval = push_period;
#else
            pushTimer.Interval = push_period;
#endif
            pushTimer.AutoReset = true;
            pushTimer.Elapsed += PushTimer_Elapsed;
            pushTimer.Start();


            dailyPushTimer.Interval = daily_push_period;
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

                var subs = await statApi.GetNoFeedbackFollowers(GeoTag, date_from, date_to);

                foreach (var subscriber in subs)
                {

                    double lastPushSendHours = (subscriber.push_send_hours != null) ? (double)subscriber.push_send_hours : 0;
                    double lastPushDeliveredHours = (subscriber.push_delivered_hours != null) ? (double)subscriber.push_delivered_hours : 0;
                    double lastPushHours = Math.Max(lastPushSendHours, lastPushDeliveredHours);

                    double Tc = subscriber.time_after_subscribe;
                    double Tp = lastPushHours;
                    double Tl = subscriber.time_diff_last_push_subscr;

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
                            await statApi.MarkFollowerWasPushed(GeoTag, id, pushmessage.TimePeriod, false);
                            throw;
                            //logger.err($"PUSH: user {subscriber.tg_user_id} NOT pushed with {pushmessage.TimePeriod} hour message {ex.Message}");
                        }

                    }
                    else
                    {
                        //No messages for user yet
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

            int sent = 0;
            int delivered = 0;

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
                    await message.Send(1784884123, bot);
                    cntr++;
                }
                else
                    cntr = 0;
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
                logger.inf_urgent($"{GeoTag} DAILY_PUSH: delivered {delivered} of {sent}");
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"{GeoTag} Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
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
                        if (Greetings.HelloMessage != null)
                            await bot.SendTextMessageAsync(
                                     chatJoinRequest.From.Id,
                                     text: Greetings.HelloMessage.Text,
                                     replyMarkup: Greetings.HelloMessage.ReplyMarkup,
                                     entities: Greetings.HelloMessage.Entities,
                                     disableWebPagePreview: true,
                                     cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.err(ex.Message);
                    }
                    await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    logger.inf_urgent($"{GeoTag} APPROVED({++appCntr}) {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");

                }
                else
                {
                    logger.inf_urgent($"{GeoTag} DECLINED({++decCntr}) {chatJoinRequest.Chat.Id} {chatJoinRequest.From.Id} {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} {chatJoinRequest.From.Username} {tags}");
                    await bot.DeclineChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    await statApi.MarkFollowerWasDeclined(GeoTag, chatJoinRequest.Chat.Id);
                }
            }
        }
        protected virtual async Task processChatMember(Update update, CancellationToken cancellationToken)
        {
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
#if CAPI_RELEASE || CAPI_DEBUG
                                logger.inf_urgent("DB+");
#endif                             
                                var linkToRevoke = await leadsGenerator.MakeFBLead(member.InviteLink.InviteLink,
                                                                                   firstname: follower.firstname,
                                                                                   lastname: follower.lastname); //return invite link to revoke

                                await linksProcessor.Revoke(ChannelID, link);

#if CAPI_RELEASE || CAPI_DEBUG
                                logger.inf_urgent("Lead+");
#endif

                                var nextLink = await linksProcessor.Generate(ChannelID);
#if CAPI_RELEASE || CAPI_DEBUG
                                logger.inf_urgent("NextLink+");
#endif
                            }
                        }
                        break;

                    case ChatMemberStatus.Left:

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
                        }
                        catch (Exception ex)
                        {
                            logger.err(ex.Message);
                        }

                        follower.is_subscribed = false;
                        followers.Add(follower);
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
            }

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
            leadsGenerator = LeadsGeneratorFactory.Create(GeoTag, LeadType, trackApi);

            //if (ChannelID != null)
            //    linksProcessor.Generate(ChannelID, 20).Wait();

            linksProcessor.StartLinkNumberControl(ChannelID, cts);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.ChatJoinRequest, UpdateType.ChatMember, UpdateType.MyChatMember }
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
