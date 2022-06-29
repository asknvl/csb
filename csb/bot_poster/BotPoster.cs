using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace csb.bot_poster
{
    public class BotPoster : IUpdateObserver
    {
        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
        Client bot;

        Messages_Chats chats;
        #endregion

        #region properties
        public string Token { get; set; }

        public long OutputChannelID { get; set; }
        #endregion

        string Config(string what)
        {
            switch (what)
            {
                case "api_id": return globals.api_id;
                case "api_hash": return globals.api_hash;
                case "bot_token": return Token;
                case "session_pathname":return "bot.session";
                default: return null;
            }
        } 

        public BotPoster(string token)
        {
            Token = token;
            
            bot = new Client(what => Config(what));

            bot.Update += Bot_Update;
        }

        public async Task Start()
        {
            var bt = await bot.LoginBotIfNeeded();         
         
        }

        private void Bot_Update(TL.IObject update)
        {            
        }

        public void Update(IObject u)
        {

            if (u is not UpdatesBase updates)
                return;
            foreach (var update in updates.UpdateList)
            {
                switch (update)
                {
                    case UpdateNewMessage unm:
                        Message message = (Message)unm.message;                        
                        Debug.WriteLine(message.message);
                        break;
                }
            }
            //throw new NotImplementedException();
        }
    }
}
