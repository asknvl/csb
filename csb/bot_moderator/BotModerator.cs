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
        #endregion

        #region properties
        [JsonProperty]
        public string GeoTag { get; protected set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonIgnore]
        public bool IsRunning { get; protected set; }
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
                AllowedUpdates = new UpdateType[] { UpdateType.ChatJoinRequest }
            };

            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            IsRunning = true;
        }

        public void Stop()
        {            
        }

        #endregion
    }
}
