using csb.chains;
using csb.moderation;
using csb.storage;
using csb.usr_push;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.users
{
    //public class User
    //{
    //    [JsonProperty]
    //    public long Id { get; set; }
    //    [JsonProperty]
    //    public string Name { get; set; }   
    //}

    public class UserManager
    {
        #region vars
        IStorage<UserManager> storage;
        CancellationToken cancellationToken;
        ITelegramBotClient bot;                
        #endregion

        #region properties
        [JsonProperty]
        List<User> users { get; set; } = new List<User>();
        #endregion

        public UserManager(ITelegramBotClient bot, CancellationToken cancellationToken)
        {
            this.bot = bot;            
            this.cancellationToken = cancellationToken;
        }

        #region public
        public void Init()
        {
            storage = new Storage<UserManager>("users.json", this);
            var t = storage.load();
            users = t.users;

            foreach (var user in users)
            {
                user.cancellationToken = cancellationToken;
                user.bot = bot;
                user.messagesProcessor = new MessagesProcessor(bot);
                user.chainsProcessor = new ChainProcessor($"{user.Id}");
                user.chainsProcessor.Load();
                user.chainsProcessor.StartAll();

                user.moderationProcessor = new ModerationProcessor($"{user.Id}");
                user.moderationProcessor.Load();
                user.moderationProcessor.StartAll();

                user.adminManager = new TGUserManager<UserAdmin>("admins.json");
                user.adminManager.StartAll();
            }
        }
        public void Add(long userId, string name)
        {
            if (!users.Any(u => u.Id == userId))
            {
                User newUser = new User()
                {
                    Id = userId,
                    bot = bot,
                    messagesProcessor = new MessagesProcessor(bot),
                    chainsProcessor = new ChainProcessor($"{userId}"),
                    moderationProcessor = new ModerationProcessor($"{userId}"),
                    adminManager = new TGUserManager<UserAdmin>("admins.json"),
                    cancellationToken = cancellationToken,                    
                    Name = name
                };

                users.Add(newUser);
                storage.save(this);
            }
        }

        public async Task UpdateCallbackQuery(CallbackQuery? query)
        {
            var user = users.FirstOrDefault(u => u.Id == query.Message.Chat.Id);
            if (user != null)
                await user.processCallbackQuery(query);
        }

        public async Task UpdateMessage(Update update)
        {
            var user = users.FirstOrDefault(u => u.Id == update.Message.Chat.Id);
            if (user != null)
                await user.processMessage(update);
        }

        public bool Check(long id)
        {
            bool res = users.Any(u => u.Id == id);
            return res;
        }

        //public List<long> GetIDs()
        //{
        //    return users.Select(u => u.Id).ToList();
        //}

        public string GetInfo()
        {
            string res = "";

            foreach (var user in users)
            {
                res += $"{user.Id}:{user.Name}\n";
            }

            return res;
        }
        #endregion
    }
}
