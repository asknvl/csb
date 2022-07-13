using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.users
{
    public class MessagesProcessor
    {

        #region vars
        Dictionary<string, Message> messages = new();
        ITelegramBotClient bot;
        #endregion

        public MessagesProcessor(ITelegramBotClient bot)
        {
            this.bot = bot;
        }

        public async Task Add(long chat, string key, Message message)
        {
            if (messages.ContainsKey(key))
            {
                var msg = messages[key];

                try
                {
                    await bot.DeleteMessageAsync(chat, msg.MessageId);
                } catch (Exception ex)
                {

                }
                messages.Remove(key);
            }

            messages.Add(key, message);
        }

        public async Task Delete(long chat, string key)
        {
            if (messages.ContainsKey(key))
            {
                try
                {
                    await bot.DeleteMessageAsync(chat, messages[key].MessageId);
                    messages.Remove(key);
                } catch { }
            }
        }

        public async Task Clear(long chat)
        {
            try
            {
                foreach (var item in messages)
                {
                    if (item.Value.Chat.Id == chat)
                        await bot.DeleteMessageAsync(chat, item.Value.MessageId);
                }
                messages.Clear();
            } catch { }
        }

        public async Task Back(long chat)
        {
            try
            {
                var msg = messages.ElementAt(messages.Count - 1);
                await bot.DeleteMessageAsync(chat, msg.Value.MessageId);
                messages.Remove(msg.Key);                
            } catch { }
        }
    }
}
