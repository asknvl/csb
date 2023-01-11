using csb.server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace csb.bot_moderator
{
    public class BotModerator : IBotModerator
    {
        #region vars
        protected ITelegramBotClient bot;
        protected CancellationTokenSource cts;

#if DEBUG
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://185.46.9.229:4000");

        public event Action<IBotModerator> ParametersUpdatedEvent;

        //protected TGStatApi statApi = new TGStatApi("http://185.46.9.229:4000");


        //protected TGStatApi statApi = new TGStatApi("http://136.243.74.153:4000");

#else
        protected ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");
        //protected TGStatApi statApi = new TGStatApi("http://136.243.74.153:4000");

        //protected TGStatApi statApi = new TGStatApi("http://192.168.72.51:4000");        

#endif
        #endregion

        #region properties
        [JsonProperty]
        public string GeoTag { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public GreetingsData Greetings { get; set; } = new();
        [JsonProperty]
        public PushData PushData { get; set; } = new();
        [JsonProperty]
        public DailyPushData DailyPushData { get; set; } = new();
        [JsonIgnore]
        public bool IsRunning { get; set; }
        [JsonIgnore]
        public uint RequestsCounter { get; set; }
        [JsonIgnore]
        public uint ApprovesCounter { get; set; }
        [JsonIgnore]
        public uint ApisendsCounter { get; set; }
        public long? ChannelID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BotModeratorLeadType? LeadType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        public BotModerator()
        {
        }

        public BotModerator(string token, string geotag)
        {
            Token = token;
            GeoTag = geotag;
        }

        #region private
        protected virtual async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            if (update.ChatMember != null)
            {
                var member = update.ChatMember;

                string info = $"{DateTime.Now} {Name}: " +
                    $"mem link={member.InviteLink?.InviteLink} " +
                    $"chat={member.Chat?.InviteLink} {member.Chat?.Username}"+
                    $"from={member.From?.FirstName} {member.From?.LastName} {member.From?.Username}" +
                    $"newmem={member.NewChatMember?.Status} " +
                    $"oldmem={member.OldChatMember?.Status} " +
                    $"date={member.Date}";

                Console.WriteLine(info);
            }

            if (update.ChatJoinRequest != null)
            {
                try
                {
                    RequestsCounter++;

                    var chatJoinRequest = update.ChatJoinRequest;                    
                    //Console.WriteLine($"join request: {Name} from {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName}");
                    bool res = await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);
                    //Console.WriteLine("Result=" + res);

                    if (res)
                        ApprovesCounter++;

                    if (res)
                    {
                        List<Follower> followers = new();
                        followers.Add(new Follower()
                        {
                            tg_chat_id = chatJoinRequest.Chat.Id,
                            tg_user_id = chatJoinRequest.From.Id,
                            username = chatJoinRequest.From.Username,
                            firstname = chatJoinRequest.From.FirstName,
                            lastname = chatJoinRequest.From.LastName,
                            invite_link = chatJoinRequest.InviteLink.InviteLink,
                            tg_geolocation = GeoTag

                        });
                        await statApi.UpdateFollowers(followers);
                        ApisendsCounter++;
                    }

                    string info = $"{DateTime.Now} {Name}: req {RequestsCounter.ToString().PadLeft(6)} link={chatJoinRequest.InviteLink.InviteLink} from {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} approved={res} approves={ApprovesCounter.ToString().PadLeft(6)} apicntr={ApisendsCounter.ToString().PadLeft(6)}";
                    Console.WriteLine(info);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"------------------------ {DateTime.Now} {ex.Message} --------------------------------");
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region public
        public virtual void Start()
        {
            if (IsRunning)
                return;

            bot = new TelegramBotClient(Token);
            User u = bot.GetMeAsync().Result;
            Name = u.Username;            

            cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.ChatJoinRequest, UpdateType.ChatMember }
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            IsRunning = true;
            Console.WriteLine($"Moderator {Name} started");
        }

        public virtual void Stop()
        {
            cts.Cancel();
            
            IsRunning = false;
            Console.WriteLine($"Moderator {Name} stopped");
        }

        #endregion
    }
}
