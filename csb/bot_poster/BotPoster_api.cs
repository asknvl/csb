using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        #endregion

        #region properties
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public long ChannelID { get; set; }
        [JsonProperty]
        public string ChannelTitle { get; set; }
        [JsonProperty]
        public string ChannelLink { get; set; }        
        [JsonIgnore]
        public bool IsRunning { get; set; }
        #endregion
        [JsonIgnore]
        public long AllowedID { get; set; }

        public BotPoster_api(string token)
        {
            Token = token;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            bot = new TelegramBotClient(Token);
            User u = bot.GetMeAsync().Result;
            Name = u.Username;

            cts = new CancellationTokenSource();

            mediaTimer.Interval = 5000;
            mediaTimer.AutoReset = false;
            mediaTimer.Elapsed += MediaTimer_Elapsed;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            Console.WriteLine(Name + "started");

            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            mediaTimer?.Dispose();
            cts?.Cancel();
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {

            if (update == null)
                return;

            if (update.Message == null)
                return;

            var message = update.Message;

            if (message.Chat.Id != AllowedID)
                return;

            string? text;
            MessageEntity[]? entities;

            if (message.ReplyMarkup != null)
                swapMarkupLink(message.ReplyMarkup, ChannelLink);

            Console.WriteLine(Name + " " + message.Text);

            try
            {

                switch (message.Type)
                {

                    case MessageType.Photo:

                        InputMediaPhoto imp = new InputMediaPhoto(new InputMedia(message.Photo[0].FileId));

                        if (!ChannelLink.Equals("0"))
                        {
                            imp.Caption = swapTextLink(message.Caption, ChannelLink);
                            imp.CaptionEntities = filterEntities(message.CaptionEntities);
                        } else
                        {

                            (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                            imp.Caption = text;
                            imp.CaptionEntities = entities;
                        }

                        if (message.MediaGroupId == null)
                        {
                            InputMediaDocument doc = new InputMediaDocument(imp.Media);

                            if (!ChannelLink.Equals("0"))
                            {
                                doc.Caption = swapTextLink(message.Caption, ChannelLink);
                                doc.CaptionEntities = filterEntities(message.CaptionEntities);
                            } else
                            {
                                (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                                doc.Caption = text;
                                doc.CaptionEntities = entities;
                            }

                            await bot.SendPhotoAsync(ChannelID,
                                doc.Media,
                                caption: doc.Caption,
                                captionEntities: doc.CaptionEntities);
                            //await bot.CopyMessageAsync(ChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                            break;
                        }

                        if (!mediaGroupId.Equals(message.MediaGroupId) && mediaList.Count > 1)
                        {
                            await bot.SendMediaGroupAsync(
                               chatId: ChannelID,
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
                        if (!ChannelLink.Equals("0"))
                        {
                            imv.Caption = swapTextLink(message.Caption, ChannelLink);
                            imv.CaptionEntities = filterEntities(message.CaptionEntities);
                        } else
                        {

                            (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                            imv.Caption = text;
                            imv.CaptionEntities = entities;
                        }

                        if (message.MediaGroupId == null)
                        {
                            InputMediaDocument doc = new InputMediaDocument(imv.Media);

                            if (!ChannelLink.Equals("0"))
                            {
                                doc.Caption = swapTextLink(message.Caption, ChannelLink);
                                doc.CaptionEntities = filterEntities(message.CaptionEntities);
                            } else
                            {

                                (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                                doc.Caption = text;
                                doc.CaptionEntities = entities;
                            }

                            await bot.SendVideoAsync(ChannelID,
                                doc.Media,
                                caption: doc.Caption,
                                captionEntities: doc.CaptionEntities);
                            //await bot.CopyMessageAsync(ChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                            break;
                        }


                        if (!mediaGroupId.Equals(message.MediaGroupId) && mediaList.Count > 1)
                        {
                            var s = await bot.SendMediaGroupAsync(
                               chatId: ChannelID,
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

                    case MessageType.Text:
                        await postTextAndWebPage(message, cancellationToken);
                        break;

                    default:
                        await bot.CopyMessageAsync(ChannelID, message.Chat, message.MessageId, null, null, message.Entities, null, null, null, null, message.ReplyMarkup, cancellationToken);
                        break;

                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void swapMarkupLink(InlineKeyboardMarkup markup, string newlink)
        {
            foreach (var button in markup.InlineKeyboard)
            {
                foreach (var item in button)
                {
                    item.Url = $"http://t.me/{newlink.Replace("@", "")}";
                }
            }
        }


        (string?, MessageEntity[]?) getUpdatedText(string text, MessageEntity[]? entities)
        {
            string? tres = null;
            MessageEntity[]? eres = null;

            if (text == null)
                return (tres, eres);

            string[] patterns = {
                @"[@][[a-zA-Z0-9_]{5,32}", //@telegram
                @"t\.me\/[-a-zA-Z0-9.]+(\/\S*)?" //t.me/asdasd
            };

            List<MessageEntity> tmpEntities = entities.ToList();


            int length = 0;

            foreach (var pattern in patterns)
            {
                Regex regex = new Regex(pattern);
                var mathces = regex.Matches(text);

                foreach (Match match in mathces)
                {
                    text = text.Replace(match.Value, "");

                    if (tmpEntities != null)
                    
                    {
                        tmpEntities = tmpEntities.OrderBy(e => e.Offset).ToList();
                        int position = match.Index;
                        var found = tmpEntities.Where(e => e.Offset == position).ToList();
                        try
                        {

                            if (found != null)
                            {
                                var index = tmpEntities.IndexOf(found[0]);
                                length += found[0].Length;
                                tmpEntities.RemoveAll(e => e.Offset == position);
                                for (int i = index; i < tmpEntities.Count; i++)
                                {
                                    tmpEntities[i].Offset -= length;
                                }
                            }
                        } catch ( Exception ex)
                        {

                        }                
                                                
                    }
                    
                }
                
            }

            if (tmpEntities != null)
                eres = tmpEntities.ToArray();

            tres = text;

            return (tres, eres);
        }

        string? swapTextLink(string text, string newlink)
        {
            if (text == null)
                return null;

            string pattern = @"[@][[a-zA-Z0-9_]{5,32}";
            Regex regex = new Regex(pattern);
            var m = regex.Matches(text);
            foreach (Match item in m)
            {
                

                if (newlink == "0")
                    text = text.Replace(item.Value, "");
                else
                    text = text.Replace(item.Value, newlink);
            }

            string res = text;

            pattern = @"t\.me\/[-a-zA-Z0-9.]+(\/\S*)?";
            regex = new Regex(pattern);
            m = regex.Matches(text);
            foreach (Match item in m)
            {
                Console.WriteLine(item.Index);
                text = text.Replace(item.Value, "");
            }

            res = text;
            return res;
        }



        MessageEntity[]? filterEntities(MessageEntity[] input)
        {
            if (input == null)
                return null;

            var entities = input;
            var res = new List<MessageEntity>();
            try
            {
                foreach (var item in entities)
                    if (item.Type != MessageEntityType.TextLink && item.Type != MessageEntityType.Url)
                    {
                        res.Add(item);
                    }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return res.ToArray();
        }

        async Task postTextAndWebPage(Message message, CancellationToken cts)
        {
            string text = message.Text;
            int tagLenCntr = 0;
            string insertUrl = "";
            bool needPreview = false;

            if (message.Entities != null)
            {
                foreach (var item in message.Entities)
                {
                    switch (item.Type)
                    {
                        case MessageEntityType.Italic:
                            insertTag("i", item.Offset, item.Length, ref text, ref tagLenCntr);
                            break;
                        case MessageEntityType.Bold:
                            insertTag("b", item.Offset, item.Length, ref text, ref tagLenCntr);
                            break;
                        case MessageEntityType.Underline:
                            insertTag("u", item.Offset, item.Length, ref text, ref tagLenCntr);
                            break;
                        case MessageEntityType.Strikethrough:
                            insertTag("s", item.Offset, item.Length, ref text, ref tagLenCntr);
                            break;
                        case MessageEntityType.TextLink:

                            if (item.Url.Contains("jpg") || item.Url.Contains("jpeg") || item.Url.Contains(""))
                            {

                            } else
                                if (item.Url.Contains(""))
                            {
                                item.Url = "";
                            }

                            //TODO
                            break;

                        case MessageEntityType.Url:
                            break;
                    }
                }

                var wp = message.Entities.FirstOrDefault(o => o.Url != null);
                if (wp != null)
                {
                    string webPage = wp.Url;
                    var u = "\"" + webPage + "\"";
                    insertUrl = "<a href=" + u + ">&#8288;</a>";
                    needPreview = true;
                }
            }

            var t = swapTextLink(text, ChannelLink) + insertUrl;

            await bot.SendTextMessageAsync(
            //chatId: channelName,
            chatId: ChannelID,
            text: t,
            disableWebPagePreview: !needPreview,
            replyMarkup: message.ReplyMarkup,
            parseMode: ParseMode.Html,
            cancellationToken: cts);
        }

        void insertTag(string tag, int offset, int length, ref string s, ref int tagLengtCntr)
        {
            string startTeg = $"<{tag}>";
            s = s.Insert(offset + tagLengtCntr, startTeg);
            tagLengtCntr += startTeg.Length;
            string stopTeg = $"</{tag}>";
            s = s.Insert(offset + length + tagLengtCntr, stopTeg);
            tagLengtCntr += stopTeg.Length;
        }

        void insertTagUrl(int offset, int length, ref string s, ref int tagLengtCntr, string url)
        {
            string startTeg = $"<a href=\"{url}\">";
            s = s.Insert(offset + tagLengtCntr, startTeg);
            tagLengtCntr += startTeg.Length;
            string stopTeg = $"</a>";
            s = s.Insert(offset + length + tagLengtCntr, stopTeg);
            tagLengtCntr += stopTeg.Length;
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

        private async void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await bot.SendMediaGroupAsync(
                    chatId: ChannelID,
                    media: mediaList,
                    cancellationToken: cts.Token);

            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //IsRunning = false;
            } finally
            {
                mediaList.Clear();
            }
        }

        public override string ToString()
        {
            return $"{Name}:\n{Token}\n{ChannelTitle}\n{ChannelLink}";
        }
    }
}
