using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using csb.bot_poster;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.messaging
{
    public abstract class DailyPushMessageBase
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public Message Message { get; set; }

        public abstract void MakeAutochange(List<AutoChange> autoChanges);
        public abstract Task Send(long id, ITelegramBotClient bot);

    }
}
