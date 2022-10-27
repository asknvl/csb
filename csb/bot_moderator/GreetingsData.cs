using Newtonsoft.Json;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_moderator
{
    public class GreetingsData
    {
        [JsonProperty]
        public TextMessage? HelloMessage { get; set; } = new();
        [JsonProperty]
        public TextMessage? ByeMessage { get; set; } = new();
    }
}
