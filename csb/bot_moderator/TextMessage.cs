using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_moderator
{
    public class TextMessage
    {
        [JsonProperty]
        public string? Text { get; set; } = null;
        [JsonProperty]
        public MessageEntity[]? Entities { get; set; } = null;
        [JsonProperty]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; } = null;
    }
}
