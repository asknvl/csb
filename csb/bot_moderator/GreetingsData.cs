using csb.messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_moderator
{
    public class GreetingsData
    {
        JoinMessage joinMessage = new();
        [JsonProperty]
        public JoinMessage? HelloMessage {
            get => joinMessage;
            set
            {
                joinMessage = value;
                updatePushStartText();                
            }
        }
        //[JsonProperty]
        //public List<JoinMessage> JoinMessages { get; set; } = new();
        [JsonProperty]
        public string PushStartEmoji { get; set; } = "🔥";
        [JsonProperty]
        public string PushStartText { get; set; }

        bool usePushStartButton = true;
        [JsonProperty]
        public bool UsePushStartButton
        {
            get => usePushStartButton;
            set
            {
                usePushStartButton = value;
                updatePushStartText();
            }
        }

        [JsonProperty]
        public JoinMessage? PreJoinMessage { get; set; } = new();
        [JsonProperty]
        public TextMessage? HelloMessageReply { get; set; } = new();
        [JsonProperty]
        public TextMessage? ByeMessage { get; set; } = new();

        public void DeleteHelloMessage()
        {
            HelloMessage.Clear();
            HelloMessage = new();
        }

        public void DeletePreJoinMessage()
        {
            PreJoinMessage.Clear();
            PreJoinMessage = new();
        }

        public event Action OnHelloMessageSetEvent;


        #region helpers
        string getPushButtonText(string message)
        {
            string res = null;

            string pattern = @"/[a-zA-Zа-яА-Я]+(?:\s|$)";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(message);

            if (match.Success)
            {
                res = match.Value;
                res = res.Replace("\n", "").Replace("/", "").Trim();
                res = $"{PushStartEmoji}{res}{PushStartEmoji}";
            }

            return res;
        }

        void updatePushStartText()
        {
            try
            {

                PushStartText = getPushButtonText(HelloMessage?.Message?.Text);
            }
            catch (Exception ex)
            {
                PushStartText = null;
            }
        }
        #endregion
    }
}
