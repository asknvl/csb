using System;
using System.Collections.Generic;
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
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Message Message { get; set; }

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
                        item.Url = item.Url.Replace(autochange.OldText, autochange.NewText).Replace("@", "");
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
            InputMediaPhoto imp = new InputMediaPhoto(new InputMedia(Message.Photo[0].FileId));
           
            imp.Caption = Message.Caption;
            imp.CaptionEntities = Message.CaptionEntities;

            InputMediaDocument doc = new InputMediaDocument(imp.Media);

            await bot.SendPhotoAsync(id,
                    doc.Media,
                    caption: imp.Caption,
                    replyMarkup: Message.ReplyMarkup,
                    captionEntities: imp.CaptionEntities);

        }
        #endregion

        public virtual void MakeAutochange(List<AutoChange> autoChanges)
        {

            if (Message.ReplyMarkup != null)
                swapMarkupLink(Message.ReplyMarkup, autoChanges);

            switch (Message.Type)
            {
                case MessageType.Text:
                    (Message.Text, Message.Entities) = autoChange(Message.Text, filterEntities(Message.Entities, autoChanges), autoChanges);
                    break;

                case MessageType.Video:
                    (Message.Text, Message.Entities) = autoChange(Message.Text, filterEntities(Message.Entities, autoChanges), autoChanges);
                    break;

            }
        }

        public virtual async Task Send(long id, ITelegramBotClient bot)
        {
            await Task.Run(async () => { 
                switch (Message.Type)
                {
                    case MessageType.Text:
                        await sendTextMessage(id, bot);
                        break;

                    case MessageType.Photo:
                        await sendPhotoMessage(id, bot);
                        break;

                    case MessageType.Video:
                        break;
                }
            });
        }

    }
}
