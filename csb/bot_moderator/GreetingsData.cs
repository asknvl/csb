using Newtonsoft.Json;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_moderator
{

    public class Button {
        [JsonProperty]
        public string? Name { get; set; } = null;
        [JsonProperty]
        public string? Link { get; set; } = null;
    }

    public class TextMessage
    {
        [JsonProperty]
        public string? Text { get; set; } = null;
        [JsonProperty]
        public MessageEntity[]? Entities { get; set; } = null;
        [JsonProperty]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; } = null;        
    }

    public class GreetingsData
    {
        [JsonProperty]
        public TextMessage? HelloMessage { get; set; } = new();
        [JsonProperty]
        public TextMessage? ByeMessage { get; set; } = new();
    }
}
