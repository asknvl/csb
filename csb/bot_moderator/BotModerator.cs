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
        ITelegramBotClient bot;
        CancellationTokenSource cts;

#if DEBUG
        TGStatApi statApi = new TGStatApi("http://185.46.9.229:4000");
#else
        //TGStatApi statApi = new TGStatApi("http://136.243.74.153:4000");
        TGStatApi statApi = new TGStatApi("http://192.168.72.51:4000");        

#endif
        #endregion

        #region properties
        [JsonProperty]
        public string GeoTag { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonIgnore]
        public bool IsRunning { get; set; }
        [JsonIgnore]
        public uint RequestsCounter { get; set; }
        public uint ApprovesCounter { get; set; }
        public uint ApisendsCounter { get; set; }
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
        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

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
                        await statApi.AddFollowers(followers);
                        ApisendsCounter++;
                    }

                    string info = $"{DateTime.Now} {Name}: req {RequestsCounter.ToString().PadLeft(6)} from {chatJoinRequest.From.Username} approved={res} approves={ApprovesCounter.ToString().PadLeft(6)} apicntr={ApisendsCounter.ToString().PadLeft(6)}";
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
        public void Start()
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

        public void Stop()
        {
            cts.Cancel();
            IsRunning = false;
            Console.WriteLine($"Moderator {Name} stopped");
        }

        #endregion
    }
}
