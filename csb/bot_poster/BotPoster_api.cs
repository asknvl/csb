using csb.server;
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
        #region const
        string[] replace_patterns = {                        
            @"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", //url
            @"\S*\.*ru",
            @"(Подписаться на )\w*\s|S",
            @"(Подписаться на )\w*.*",
            @"[@][[a-zA-Z0-9_]{5,32}", //@telegram
            @"t\.me\/[-a-zA-Z0-9.]+(\/\S*)?", //t.me/asdasd
        };


        string[] replace_patterns_change_links = {
            @"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", //url
            @"\S*\.*ru",
            @"(Подписаться на )\w*\s|S",
            @"(Подписаться на )\w*.*",            
            @"t\.me\/[-a-zA-Z0-9.]+(\/\S*)?", //t.me/asdasd
        };
        #endregion

        #region vars
        ITelegramBotClient bot;
        CancellationTokenSource cts;
        string mediaGroupId = "";
        List<IAlbumInputMedia> mediaList = new();
        System.Timers.Timer mediaTimer;
        #endregion

        #region properties
        [JsonProperty]
        public string GeoTag { get; set; }
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public long ChannelID { get; set; }
        [JsonProperty]
        public string ChannelTitle { get; set; }
        [JsonProperty]
        public string VictimLink { get; set; }
        [JsonProperty]
        public string ChannelLink { get; set; }
        [JsonProperty]
        public List<string> ReplacedWords { get; set; } = new();
        [JsonProperty]
        public List<AutoChange> AutoChanges { get; set; } = new();
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

            mediaTimer = new System.Timers.Timer();
            mediaTimer.Interval = 5000;
            mediaTimer.AutoReset = false;
            mediaTimer.Elapsed += MediaTimer_Elapsed;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            Console.WriteLine(Name + "output bot" + "started");

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
                            
                            (imp.Caption, imp.CaptionEntities) = autoChange(message.Caption, filterEntities(message.CaptionEntities), AutoChanges);

                        } else
                        {

                            (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                            imp.Caption = text;
                            imp.CaptionEntities = entities;                            
                        }

                        if (message.MediaGroupId == null)
                        {
                            InputMediaDocument doc = new InputMediaDocument(imp.Media);

                            await bot.SendPhotoAsync(ChannelID,
                                doc.Media,
                                caption: imp.Caption,
                                replyMarkup:message.ReplyMarkup,
                                captionEntities: imp.CaptionEntities);
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
                            (imv.Caption, imv.CaptionEntities) = autoChange(message.Caption, filterEntities(message.CaptionEntities), AutoChanges);
                        }
                        else
                        {

                            (text, entities) = getUpdatedText(message.Caption, message.CaptionEntities);
                            imv.Caption = text;
                            imv.CaptionEntities = entities;
                        }

                        if (message.MediaGroupId == null)
                        {
                            InputMediaDocument doc = new InputMediaDocument(imv.Media);

                            await bot.SendVideoAsync(ChannelID,
                                doc.Media,
                                caption: imv.Caption,
                                replyMarkup: message.ReplyMarkup,
                                captionEntities: imv.CaptionEntities);
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
                        break;

                    case MessageType.Document:

                        text = message.Caption;
                        entities = message.CaptionEntities;

                        if (!ChannelLink.Equals("0"))
                        {
                            (text, entities) = autoChange(text, filterEntities(entities), AutoChanges);

                        } else
                        {
                            (text, entities) = getUpdatedText(text, entities);
                        }

                        InputMedia idoc = new InputMedia(message.Document.FileId);
                        await bot.SendDocumentAsync(
                                chatId: ChannelID,
                                idoc,
                                caption: text,
                                captionEntities: entities);

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

            List<MessageEntity>? tmpEntities = entities?.ToList();

            foreach (var replaced in ReplacedWords)
            {
                string spaces = new string(' ', replaced.Length);
                text = text.Replace(replaced, spaces);
            }

            int length = 0;
            var patterns = replace_patterns.ToList();

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
            {
                tmpEntities.RemoveAll((e) => e.Type == MessageEntityType.TextLink);
                eres = tmpEntities.ToArray();
            }

            tres = text;

            return (tres, eres);
        }

        string? autoChange(string text)
        {

            if (text == null)
                return null;

            string res = text;

            foreach (var autochange in AutoChanges)
            {
                res = res.Replace(autochange.OldText, autochange.NewText);
            }

            return res;
        }    

        (string, MessageEntity[]? entities) autoChange(string text, MessageEntity[]? entities, List<AutoChange> autoChanges)
        {

            AutoChange pmChange = new AutoChange()
            {
                OldText = VictimLink,
                NewText = ChannelLink
            };

            if (!autoChanges.Any(a => a.OldText.Equals(pmChange.OldText) && a.NewText.Equals(pmChange.NewText)))
                autoChanges.Add(pmChange);

            string resText = text;
            List<MessageEntity>? resEntities = entities?.ToList();

            if (text == null)
                return (null, null);

            foreach (var autochange in autoChanges)
            {
                resEntities = resEntities?.OrderBy(e => e.Offset).ToList();
                int indexReplace = resText.IndexOf(autochange.OldText);
                if (indexReplace == -1)
                    continue;

                resText = resText.Replace(autochange.OldText, autochange.NewText);

                if (resEntities != null)
                {
                    int delta = autochange.NewText.Length - autochange.OldText.Length;
                    var found = resEntities.Where(e => e.Offset == indexReplace).ToList();

                    foreach (var item in found)
                    {
                        int ind = resEntities.IndexOf(item);
                        resEntities[ind].Length += delta;
                    }

                    if (found != null && found.Count > 0)
                    {
                        var indexEntity = resEntities.IndexOf(found[0]);
                        for (int i = indexEntity + 1; i < resEntities.Count; i++)
                        {
                            if (resEntities[i].Offset > indexReplace)
                                resEntities[i].Offset += delta;
                        }
                    }

                }
            }

            return (resText, resEntities?.ToArray());
        }

        MessageEntity[]? filterEntities(MessageEntity[] input)
        {
            if (input == null)
                return null;

            var entities = input;
            try
            {
                foreach (var item in entities)
                {

                    switch (item.Type)
                    {
                        case MessageEntityType.TextLink:
                            item.Url = autoChange(item.Url);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return entities;
        }

        async Task postTextAndWebPage(Message message, CancellationToken cts)
        {
            string text = message.Text;
            
            MessageEntity[]? messageEntities = null;

            string t = text;
            if (!ChannelLink.Equals("0"))
            {
                (t, messageEntities) = autoChange(t, message.Entities, AutoChanges);

            } else
            {
                MessageEntity[] e;
                (t, e) = getUpdatedText(text, null);
            }


            bool disablePreview = true;
            if (messageEntities != null && messageEntities.Length > 0)
            {
                disablePreview = messageEntities[0].Type != MessageEntityType.TextLink;
            }

           
            //bool disablePreview = false;

            //if (!string.IsNullOrEmpty(GeoTag) && GeoTag.Contains("BRAA"))
            //    disablePreview = true;

            await bot.SendTextMessageAsync(            
            chatId: ChannelID,
            text: t,
            entities: filterEntities(messageEntities),
            disableWebPagePreview: disablePreview,
            replyMarkup: message.ReplyMarkup,            
            cancellationToken: cts);
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
            return $"{Name}:\n{ChannelTitle}\n{VictimLink}\n{ChannelLink}";
        }
    }
}
