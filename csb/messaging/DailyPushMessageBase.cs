using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using csb.bot_poster;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.messaging
{
    public abstract class DailyPushMessageBase
    {
        
        #region properties
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Message Message { get; set; }
        [JsonProperty]
        public string FilePath { get; set; }
        [JsonIgnore]
        public string fileId { get; set; } = null;
        #endregion

        public DailyPushMessageBase()
        {
        }

        #region helpers
        protected (string, MessageEntity[]? entities) autoChange(string text, MessageEntity[]? entities, List<AutoChange> autoChanges)
        {
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

        string? autoChange(string text, List<AutoChange> autoChanges)
        {

            if (text == null)
                return null;

            string res = text;

            foreach (var autochange in autoChanges)
            {
                res = res.Replace(autochange.OldText, autochange.NewText);
            }

            return res;
        }

        MessageEntity[]? filterEntities(MessageEntity[] input, List<AutoChange> autoChanges)
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
                            item.Url = autoChange(item.Url, autoChanges);
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

        void swapMarkupLink(InlineKeyboardMarkup markup, List<AutoChange> autoChanges)
        {
            foreach (var button in markup.InlineKeyboard)
            {
                foreach (var item in button)
                {
                    foreach (var autochange in autoChanges)
                        item.Url = item.Url.Replace(autochange.OldText.Replace("@", ""), autochange.NewText.Replace("@", ""));
                }
            }
        }

        async Task<int> sendTextMessage(long id, ITelegramBotClient bot)
        {
            var m = await bot.SendTextMessageAsync(
                    chatId: id,
                    text: Message.Text,
                    entities: Message.Entities,
                    replyMarkup: Message.ReplyMarkup,
                    cancellationToken: new CancellationToken());
            return m.MessageId;
        }

        async Task<int> sendPhotoMessage(long id, ITelegramBotClient bot)
        {
            int messageId;

            if (fileId == null)
            {
                Console.WriteLine($"Message {id} fileId=null");

                using (var fileStream = System.IO.File.OpenRead(FilePath))
                {

                    var sent = await bot.SendPhotoAsync(id,
                            fileStream,
                            caption: Message.Caption,
                            replyMarkup: Message.ReplyMarkup,
                            captionEntities: Message.CaptionEntities);

                    fileId = sent.Photo.Last().FileId;
                    messageId = sent.MessageId;
                }
            } else
            {
                InputMediaPhoto imp = new InputMediaPhoto(new InputMedia(fileId));

                imp.Caption = Message.Caption;
                imp.CaptionEntities = Message.CaptionEntities;

                InputMediaDocument doc = new InputMediaDocument(imp.Media);

                var sent = await bot.SendPhotoAsync(id,
                       doc.Media,
                       caption: Message.Caption,
                       replyMarkup: Message.ReplyMarkup,
                       captionEntities: Message.CaptionEntities);
                messageId = sent.MessageId;
            }
            return messageId;
        }

        async Task<int> sendVideoMessage(long id, ITelegramBotClient bot)
        {
            int messageId;
            if (fileId == null)
            {
                Console.WriteLine($"Message {id} fileId=null");

                using (var fileStream = System.IO.File.OpenRead(FilePath))
                {

                    var sent = await bot.SendVideoAsync(id,
                            fileStream,
                            caption: Message.Caption,
                            replyMarkup: Message.ReplyMarkup,
                            captionEntities: Message.CaptionEntities);

                    fileId = sent.Video.FileId;
                    messageId = sent.MessageId;
                }
            }
            else
            {
                InputMediaVideo imv = new InputMediaVideo(new InputMedia(fileId));

                imv.Caption = Message.Caption;
                imv.CaptionEntities = Message.CaptionEntities;

                InputMediaDocument doc = new InputMediaDocument(imv.Media);

                var sent = await bot.SendVideoAsync(id,
                       doc.Media,
                       caption: Message.Caption,
                       replyMarkup: Message.ReplyMarkup,
                       captionEntities: Message.CaptionEntities);
                messageId = sent.MessageId;
            }

            return messageId;
        }

        async Task<int> sendDocumentMessage(long id, ITelegramBotClient bot)
        {
            int messageId;
            if (fileId == null)
            {
                Console.WriteLine($"Message {id} fileId=null");

                using (var fileStream = System.IO.File.OpenRead(FilePath))
                {

                    InputMedia idoc = new InputMedia(fileStream, Path.GetFileName(FilePath));
                    
                    var sent = await bot.SendDocumentAsync(id,
                        idoc,
                        caption: Message.Caption,
                        replyMarkup: Message.ReplyMarkup,
                        captionEntities: Message.CaptionEntities);

                    fileId = sent.Document.FileId;
                    messageId = sent.MessageId;
                }
            } else
            {

                InputMedia doc = new InputMedia(fileId);

                var sent = await bot.SendDocumentAsync(id,
                    doc,
                    caption: Message.Caption,
                    replyMarkup: Message.ReplyMarkup,
                    captionEntities: Message.CaptionEntities);
                messageId = sent.MessageId;
            }

            return messageId;
        }

        async Task<int> send(long id, ITelegramBotClient bot)
        {
            int messageId;
            switch (Message.Type)
            {
                case MessageType.Text:
                    messageId =  await sendTextMessage(id, bot);
                    break;

                case MessageType.Photo:
                    messageId = await sendPhotoMessage(id, bot);
                    break;

                case MessageType.Video:
                    messageId = await sendVideoMessage(id, bot);
                    break;

                case MessageType.Document:
                    messageId = await sendDocumentMessage(id, bot);
                    break;

                default:
                    messageId = 0;
                    break;
            }

            return messageId;
        }

        #endregion

        public static async Task<DailyPushMessage> Create(long id, ITelegramBotClient bot, Message pattern, string chainName)
        {
            DailyPushMessage res = new DailyPushMessage();
            res.Message = pattern;

            string fileId = null;
            Telegram.Bot.Types.File fileInfo;
            string filePath = null;

            await Task.Run(async () => { 

                switch (res.Message.Type)
                {
                    case MessageType.Text:
                        break;
                    case MessageType.Photo:
                        fileId = res.Message.Photo.Last().FileId;
                        break;
                    case MessageType.Video:
                        fileId = res.Message.Video.FileId;
                        break;
                    case MessageType.Document:
                        fileId = res.Message.Document.FileId;
                        break;
                }

                if (fileId != null)
                {
                    fileInfo = await bot.GetFileAsync(fileId);
                    filePath = fileInfo.FilePath;

                    var fileName = filePath.Split('/').Last();

                    string destinationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "chains", $"{id}", chainName);
                    if (!Directory.Exists(destinationFilePath))
                        Directory.CreateDirectory(destinationFilePath);

                    destinationFilePath = Path.Combine(destinationFilePath, fileName);

                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await bot.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);

                    res.FilePath = destinationFilePath;
                }

            });

            return res;

        }

        public virtual void MakeAutochange(List<AutoChange> autoChanges)
        {

            if (Message.ReplyMarkup != null)
                swapMarkupLink(Message.ReplyMarkup, autoChanges);

            switch (Message.Type)
            {
                case MessageType.Text:
                    (Message.Text, Message.Entities) = autoChange(Message.Text, filterEntities(Message.Entities, autoChanges), autoChanges);
                    break;

                case MessageType.Photo:
                case MessageType.Video:
                case MessageType.Document:
                    (Message.Caption, Message.CaptionEntities) = autoChange(Message.Caption, filterEntities(Message.CaptionEntities, autoChanges), autoChanges);
                    break;

            }
        }

        public virtual async Task<int> Send(long id, ITelegramBotClient bot)
        {
            int messageId = 0;

            await Task.Run(async () => {

                try
                {
                    messageId = await send(id, bot);

                } catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("wrong file"))
                    {
                        Console.WriteLine("Resending with fileId = null");
                        fileId = null;
                        messageId = await send(id, bot);
                    } else
                        throw;
                }
            });

            return messageId;
        }

        public DailyPushMessage Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<DailyPushMessage>(serialized);
        }

    }
}
