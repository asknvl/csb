using csb.bot_poster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TL;
using WTelegram;

namespace csb.usr_listener
{
    public class UserListener
    {

        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
        Client user;        
        TL.Messages_Chats chats;
        TL.Messages_Dialogs dialogs;

        ChatBase from_chat;

        List<Contacts_ResolvedPeer> resolvedBots = new();

        MediaGroup mediaGroup = new();

        private readonly ManualResetEventSlim codeReady = new();
        #endregion

        #region properties
        public string PhoneNumber { get; set; }
        public List<string> CorrespondingBotNames { get; set; } = new();

        string vcode = "";
        public string VerifyCode {
            get => vcode;
            set
            {
                vcode = value;        
            }
        }

        public bool IsRunning { get; set; }
        #endregion

        string Config(string what)
        {

            string dir = Path.Combine(Directory.GetCurrentDirectory(), "userpool");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            switch (what)
            {
                case "api_id": return globals.api_id;
                case "api_hash": return globals.api_hash;
                case "session_pathname": return $"{dir}/{PhoneNumber}.session";
                case "phone_number": return PhoneNumber;
                //case "verification_code": /*return "65420";*/Console.Write("Code: "); return Console.ReadLine();
                
                case "verification_code":
                    NeedVerifyCodeEvent?.Invoke(PhoneNumber);                    
                    codeReady.Reset();
                    codeReady.Wait();
                    return VerifyCode;             
            
                case "first_name": return "Stevie";      // if sign-up is required
                case "last_name": return "Voughan";        // if sign-up is required
                case "password": return "secret!";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }

        }
       
        public UserListener(string phonenumber)
        {
            PhoneNumber = phonenumber;                   
        }
        
        private async void User_Update(TL.IObject u)
        {
            //NotifyObservers(update);

            if (u is not UpdatesBase updates)
                return;
            foreach (var update in updates.UpdateList)
            {

                switch (update)
                {   

                    case UpdateNewMessage unm:

                        //if (1665029284 == unm.message.Peer.ID)
                        //    return;


                        //if (1708105731 != unm.message.Peer.ID)
                        //    return;

                        Message m;
                        try
                        {
                            m = (Message)unm.message;                           
                            from_chat = chats.chats[unm.message.Peer.ID];
                        } catch (Exception ex)
                        {
                            return;
                        }

#if DEBUG
                        if (m.fwd_from != null)
                            continue;
#endif

                        //if (resolved == null)
                        //    resolved = await user.Contacts_ResolveUsername(CorrespondingBotName);

                        InputSingleMedia sm;


                        switch (m.media) {

                            case MessageMediaPhoto mmp:                                
                                mediaGroup.Update(m.grouped_id, unm.message.ID);
                                break;

                            case MessageMediaDocument mmd:
                                mediaGroup.Update(m.grouped_id, unm.message.ID);                                                               
                                break;

                            //case MessageMediaWebPage wp:
                            //    //await user.SendMessageAsync(resolved, m.message, 0, m.entities, default, true);
                            //    //await user.Messages_SendMessage(resolved, m.message, Helpers.RandomLong(), false, false, false, false, false, null, m.reply_markup, m.entities, null, null);
                            //    break;

                            default:
                                foreach (var item in resolvedBots)
                                    try
                                    {
                                        long rand = Helpers.RandomLong();                                        
                                        await user.Messages_ForwardMessages(from_chat, new[] { unm.message.ID }, new[] { rand }, item);
                                        Thread.Sleep(1000);
                                    } catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                break;

                        }
                        break;                        
                }
            }            
        }

        private async void MediaGroup_MediaReadyEvent(MediaGroup group)
        {
            foreach (var item in resolvedBots)
            {
                try
                {
                    List<long> rands = new();
                    for (int i = 0; i < group.MessageRands.Count; i++)
                        rands.Add(Helpers.RandomLong());
                    //Суперважно менять рандомные айди при рассылке многим пользоватям одного и того же
                    await user.Messages_ForwardMessages(from_chat, group.MessageIDs.ToArray(), /*group.MessageRands.ToArray()*/ rands.ToArray(), item);
                    Thread.Sleep(1000);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        #region public
        public void SetVerifyCode(string code)
        {
            VerifyCode = code;
            codeReady.Set();
        }

        public async Task AddInputChannel(string input)
        {
            string hash = "";
            string[] splt = input.Split("/");
            input = splt.Last();

            if (input.Contains("+"))
            {
                hash = input.Replace("+", "");
                var cci = await user.Messages_CheckChatInvite(hash);
                var ici = await user.Messages_ImportChatInvite(hash);
            }  else
            {
                hash = input.Replace("@", "");
                var resolved = await user.Contacts_ResolveUsername(hash); // without the @
                if (resolved.Chat is Channel channel)
                    await user.Channels_JoinChannel(channel);
            }

            //user.Channels_JoinChannel(new InputChannel())

            //await user.Channels_LeaveChannel()
            //InputChannelBase c = new InputChannel(cci.);
            //await user.Channels_JoinChannel();

            //chats = await user.Messages_GetAllChats();
            //user.Channels_JoinChannel()
        }

        public async Task ResoreInputChannels()
        {
            chats = await user.Messages_GetAllChats();
        }

        public async Task<List<(string, long)>> GetAllChannels()
        {
            List <(string, long)> res = new();
            //var chats = await user.Messages_GetAllChats();                
            await Task.Run(() => { 
                foreach (var item in chats.chats)
                {
                    if (item.Value is Channel channel)
                    {
                        res.Add(new (item.Value.Title, item.Value.ID));
                    }
                }
            });

            return res; 
        }

        public async Task LeaveChannel(string title)
        {
            //var resolved = await user.Contacts_ResolveUsername(title);           
            //var chats = await user.Messages_GetAllChats();
            var channels = chats.chats.Where(o => o.Value is Channel);
            var channel = channels.FirstOrDefault(o => o.Value.Title.Equals(title));
            var c = (Channel)channel.Value;
            await user.Channels_LeaveChannel(new InputChannel(c.ID, c.access_hash));
            chats.chats.Remove(channel.Key);
            //chats = await user.Messages_GetAllChats();                
            //await user.Channels_LeaveChannel(channel.);
        }

        public void AddCorrespondingBot(string name)
        {
            if (!CorrespondingBotNames.Contains(name)) 
                CorrespondingBotNames.Add(name);
        } 

        public void Start()
        {
            if (IsRunning)
            {
                ResoreInputChannels().Wait();
            }                

            mediaGroup = new();
            mediaGroup.MediaReadyEvent += MediaGroup_MediaReadyEvent;

            Task.Run(async () => { 
                user = new Client(Config);                           
                var usr = await user.LoginUserIfNeeded();
                chats = await user.Messages_GetAllChats();
                dialogs = await user.Messages_GetAllDialogs();
                foreach (var item in CorrespondingBotNames)
                {
                    resolvedBots.Add(await user.Contacts_ResolveUsername(item));
                }
                user.Update += User_Update;
                IsRunning = true;
            });
        }

        public void Stop()
        {
            user?.Dispose();            
            IsRunning = false; 
        }
        #endregion

        

        public event Action<string> NeedVerifyCodeEvent;

    }
}
