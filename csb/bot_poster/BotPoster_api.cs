using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_poster
{
    public class BotPoster_api
    {
        #region vars
        ITelegramBotClient bot;
        CancellationTokenSource cts;


        string mediaGroupId = "";
        List<IAlbumInputMedia> mediaList = new();
        System.Timers.Timer mediaTimer = new System.Timers.Timer();
        string token;
        #endregion

        #region properties
        public long OutputChannelID { get; set; }

        public string OutputBotName { get; set; }
        public bool IsRunning { get; set; }
        #endregion

        public BotPoster_api(string token)
        {           
            this.token = token;
        }

        public void Start()
        {
            bot = new TelegramBotClient(token);
            User u = bot.GetMeAsync().Result;
            OutputBotName = u.Username;           

            cts = new CancellationTokenSource();

            mediaTimer.Interval = 5000;
            mediaTimer.AutoReset = false;
            mediaTimer.Elapsed += MediaTimer_Elapsed;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            IsRunning = true;
        }
        
        public void Stop()
        {
            mediaTimer.Dispose();
            cts.Cancel();
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
          
            var message = update.Message;           

            switch (message.Type)
            {

                case MessageType.Photo:
                    InputMediaPhoto imp = new InputMediaPhoto(new InputMedia(message.Photo[0].FileId));
                    imp.Caption = message.Caption;
                    imp.CaptionEntities = message.CaptionEntities;

                    
                    if (message.MediaGroupId == null)
                    {
                        await bot.CopyMessageAsync(OutputChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                        break;
                    }                   

                    if (!mediaGroupId.Equals(message.MediaGroupId) && mediaList.Count > 1)
                    {
                        await bot.SendMediaGroupAsync(
                           chatId: OutputChannelID,
                           media: mediaList,
                            cancellationToken: cancellationToken);
                        mediaList.Clear();
                    }
                    mediaGroupId = message.MediaGroupId;

                    if (mediaList.Count == 0)                        
                        mediaTimer.Start();

                    mediaList.Add(imp);

                    break;


                case MessageType.Video:
                    InputMediaVideo imv = new InputMediaVideo(new InputMedia(message.Video.FileId));
                    imv.Caption = message.Caption;
                    imv.CaptionEntities = message.CaptionEntities;

                    if (message.MediaGroupId == null)
                    {
                        await bot.CopyMessageAsync(OutputChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                        break;
                    }
                    

                    if (!mediaGroupId.Equals(message.MediaGroupId) && mediaList.Count > 1)
                    {
                        await bot.SendMediaGroupAsync(
                           chatId: OutputChannelID,
                           media: mediaList,
                            cancellationToken: cancellationToken);
                        mediaList.Clear();
                    }
                    mediaGroupId = message.MediaGroupId;

                    if (mediaList.Count == 0)
                        mediaTimer.Start();

                    mediaList.Add(imv);


                    //await bot.CopyMessageAsync(OutputChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, new CancellationToken());
                    break;

                default:

                    //var inkm = message.ReplyMarkup;
                    //foreach (var button in inkm.InlineKeyboard)
                    //{
                    //    foreach (var item in button)
                    //    {
                    //        item.
                    //    }
                    //}


                    await bot.CopyMessageAsync(OutputChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                    break;

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
            //Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await bot.SendMediaGroupAsync(
                    chatId: OutputChannelID,
                    media: mediaList,                    
                    
                    cancellationToken: cts.Token);

                mediaList.Clear();
            } catch (Exception ex)
            {             
                IsRunning = false;
            }
        }
    }
}
