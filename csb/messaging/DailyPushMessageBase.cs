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
        #region vars
        FileStream fileStream = null;
        #endregion

        #region properties
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Message Message { get; set; }
        [JsonProperty]
        public string FilePath { get; set; }
        [JsonProperty]
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

        async Task sendTextMessage(long id, ITelegramBotClient bot)
        {
            await bot.SendTextMessageAsync(
                    chatId: id,
                    text: Message.Text,
                    entities: Message.Entities,
                    replyMarkup: Message.ReplyMarkup,
                    cancellationToken: new CancellationToken());
        }

        async Task sendPhotoMessage(long id, ITelegramBotClient bot)
        {
            if (fileId == null)
            {

                fileStream = System.IO.File.OpenRead(FilePath);

                var sent = await bot.SendPhotoAsync(id,
                        fileStream,
                        caption: Message.Caption,
                        replyMarkup: Message.ReplyMarkup,
                        captionEntities: Message.CaptionEntities);

                fileId = sent.Photo.Last().FileId;
            }
            else
            {
                InputMediaPhoto imp = new InputMediaPhoto(new InputMedia(fileId));

                imp.Caption = Message.Caption;
                imp.CaptionEntities = Message.CaptionEntities;

                InputMediaDocument doc = new InputMediaDocument(imp.Media);

                await bot.SendPhotoAsync(id,
                       doc.Media,
                       caption: Message.Caption,
                       replyMarkup: Message.ReplyMarkup,
                       captionEntities: Message.CaptionEntities);
            }

        }

        async Task sendVideoMessage(long id, ITelegramBotClient bot)
        { 
            if (fileId == null)
            {

                fileStream = System.IO.File.OpenRead(FilePath);

                var sent = await bot.SendVideoAsync(id,
                        fileStream,
                        caption: Message.Caption,
                        replyMarkup: Message.ReplyMarkup,
                        captionEntities: Message.CaptionEntities);

                fileId = sent.Video.FileId;
            }
            else
            {
                InputMediaVideo imv = new InputMediaVideo(new InputMedia(fileId));

                imv.Caption = Message.Caption;
                imv.CaptionEntities = Message.CaptionEntities;

                InputMediaDocument doc = new InputMediaDocument(imv.Media);

                await bot.SendVideoAsync(id,
                       doc.Media,
                       caption: Message.Caption,
                       replyMarkup: Message.ReplyMarkup,
                       captionEntities: Message.CaptionEntities);
            }

        }

        async Task send(long id, ITelegramBotClient bot)
        {
            switch (Message.Type)
            {
                case MessageType.Text:
                    await sendTextMessage(id, bot);
                    break;

                case MessageType.Photo:
                    await sendPhotoMessage(id, bot);
                    break;

                case MessageType.Video:
                    await sendVideoMessage(id, bot);
                    break;

                default:
                    break;
            }
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

        public virtual async Task Send(long id, ITelegramBotClient bot)
        {
            await Task.Run(async () => {

                try
                {
                    await send(id, bot);
                    
                } catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("wrong file"))
                    {
                        Console.WriteLine("Resending with fileId = null");
                        fileId = null;
                        await send(id, bot);
                    } else
                        throw;
                }
            });
        }

        public DailyPushMessage Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<DailyPushMessage>(serialized);
        }

    }
}
