using csb.bot_poster;
using Newtonsoft.Json;
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
        System.Timers.Timer timer;
        bool AllowMessagingFlag = true;
        #endregion

        #region properties
        [JsonIgnore]
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public List<string> CorrespondingBotNames { get; set; } = new();

        [JsonIgnore]
        public long ID { get; set; }

        string vcode = "";
        [JsonIgnore]
        public string VerifyCode {
            get => vcode;
            set
            {
                vcode = value;        
            }
        }
        [JsonIgnore]
        public bool IsRunning { get; set; }

        [JsonProperty]
        public List<string> FilteredWords { get; set; } = new();

        double timeinterval = 0;
        [JsonProperty]
        public double TimeInterval
        {
            get => timeinterval;
            set
            {
                timeinterval = value;
                if (timer != null)
                {
                    if (timeinterval > 0)
                    {
                        timer.Interval = 60 * timeinterval * 1000;
                        timer.Start();
                    } else
                    {
                        timer.Stop();
                        AllowMessagingFlag = true;
                    }
                }
            }
        }
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
                case "password": return "5555";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }

        }
       
        public UserListener(string phonenumber)
        {
            PhoneNumber = phonenumber;

            timer = new();
            //timer.Interval = 60 * TimeInterval * 1000;
            timer.Elapsed += (sender, e) =>
            {

                AllowMessagingFlag = true;
                Console.WriteLine($"{PhoneNumber} {DateTime.Now} AllowMessaging=" + AllowMessagingFlag);

            };
            timer.AutoReset = true;

        }

        private async void User_Update(TL.IObject u)
        {
            //NotifyObservers(update);

            
            if (!AllowMessagingFlag)
                return;

            if (u is not UpdatesBase updates)
                return;
            foreach (var update in updates.UpdateList)
            {

                switch (update)
                {

                    case UpdateNewMessage unm:

                        Message m;
                        try
                        {
                            m = (Message)unm.message;

                            //Filtering text of a message
                            if (m.media == null || m.media is MessageMediaWebPage)
                            {
                                foreach (var item in FilteredWords)
                                    if (m.message.ToLower().Contains(item.ToLower()))
                                    {
                                        Console.WriteLine($"filtered byt: {item}");
                                        return;
                                    }
                            }


                            from_chat = chats.chats[unm.message.Peer.ID];
                        } catch (Exception ex)
                        {
                            return;
                        }

#if DEBUG
                        //if (m.fwd_from != null)
                        //    continue;
#endif

                        //if (resolved == null)
                        //    resolved = await user.Contacts_ResolveUsername(CorrespondingBotName);

                        InputSingleMedia sm;


                        switch (m.media)
                        {

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
                                {
                                    try
                                    {
                                        long rand = Helpers.RandomLong();
                                        await user.Messages_ForwardMessages(from_chat, new[] { unm.message.ID }, new[] { rand }, item);
                                        //Thread.Sleep(1000);
                                    } catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                                if (TimeInterval > 0)
                                    AllowMessagingFlag = false;
                                break;

                        }
                        break;
                }
            }
        }

        private async void MediaGroup_MediaReadyEvent(MediaGroup group)
        {

            try
            {

                bool filterFlag = false;

                foreach (var id in group.MessageIDs)
                {
                    Messages_MessagesBase message = await user.GetMessages(from_chat, group.MessageIDs[0]);
                    MessageBase mb = message.Messages[0] as MessageBase;
                    Message m = mb as Message;

                    if (m != null)
                    {
                        foreach (var item in FilteredWords)
                        {
                            filterFlag = m.message.ToLower().Contains(item.ToLower());
                            if (filterFlag)
                                break;
                        }
                    }

                }

                if (filterFlag)
                {
                    return;
                }

                foreach (var item in resolvedBots)
                {
                    try
                    {
                        List<long> rands = new();
                        for (int i = 0; i < group.MessageRands.Count; i++)
                            rands.Add(Helpers.RandomLong());
                        //Суперважно менять рандомные айди при рассылке многим пользоватям одного и того же
                        await user.Messages_ForwardMessages(from_chat, group.MessageIDs.ToArray(), /*group.MessageRands.ToArray()*/ rands.ToArray(), item);
                        //Thread.Sleep(1000);
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                if (TimeInterval > 0)
                    AllowMessagingFlag = false;


            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

        public async Task<List<(string, string, long)>> GetAllChannels()
        {
            List <(string, string, long)> res = new();
            //var chats = await user.Messages_GetAllChats();                

            if (chats == null)
                throw new Exception("Цепочка не была запущена, нет информации о входных каналах");

            await Task.Run(() => { 
                foreach (var item in chats.chats)
                {
                    if (item.Value is Channel channel)
                    {
                        //string name = (((Channel)item.Value).username != null) ? ((Channel)item.Value).username : ((Channel)item.Value).Title;

                        string title =  ((Channel)item.Value).Title;
                        string username = ((Channel)item.Value).username;

                        res.Add(new (title, username, item.Value.ID));
                    }
                }
            });

            return res; 
        }

        public async Task LeaveChannel(string id)
        {
            //var resolved = await user.Contacts_ResolveUsername(title);           
            //var chats = await user.Messages_GetAllChats();

            //var channels = chats.chats.Where(o => o.Value is Channel);
            //var channel = channels.FirstOrDefault(o => o.Value.Title.Equals(title));
            //var c = (Channel)channel.Value;

            try
            {

                foreach (var item in chats.chats)
                {
                    if (item.Value is Channel)
                    {
                        var c = (Channel)item.Value;

                        //if (c.username.Equals(text))
                        //{
                        //    await user.Channels_LeaveChannel(new InputChannel(c.ID, c.access_hash));
                        //    chats.chats.Remove(item.Key);
                        //}

                        if (c.ID == long.Parse(id))
                        {
                            await user.Channels_LeaveChannel(new InputChannel(c.ID, c.access_hash));
                            chats.chats.Remove(item.Key);
                        }
                    }
                }
            } catch (FormatException ex)
            {
                throw new Exception("Неверный ID канала");
            }
            catch (Exception ex)
            {
                throw new Exception("Для удаления каналов цепочка должна быть запущена");
            }


            //var channel = chats.chats.FirstOrDefault(o => o is Channel && ((Channel)o.Value).username.Equals(title));
            //var c = (Channel)channel.Value;

            //await user.Channels_LeaveChannel(new InputChannel(c.ID, c.access_hash));
            //chats.chats.Remove(channel.Key);

            //chats = await user.Messages_GetAllChats();                
            //await user.Channels_LeaveChannel(channel.);
        }

        public async Task AddCorrespondingBot(string name)
        {
            if (!CorrespondingBotNames.Contains(name)) 
                CorrespondingBotNames.Add(name);

            resolvedBots.Clear();

            foreach (var item in CorrespondingBotNames)
            {
                if (user != null)
                {
                    resolvedBots.Add(await user.Contacts_ResolveUsername(item));
                }
            }



        } 

        public void Start()
        {
            if (IsRunning)
            {                
                ResoreInputChannels().Wait();
                StartedEvent?.Invoke(PhoneNumber);
                return;
            }                

            mediaGroup = new();

            mediaGroup.MediaReadyEvent += MediaGroup_MediaReadyEvent;

            Task.Run(async () =>
            {
                user = new Client(Config);
                var usr = await user.LoginUserIfNeeded();
                ID = usr.ID;
                chats = await user.Messages_GetAllChats();
                dialogs = await user.Messages_GetAllDialogs();
                foreach (var item in CorrespondingBotNames)
                {
                    resolvedBots.Add(await user.Contacts_ResolveUsername(item));
                }

                if (TimeInterval > 0)
                    timer?.Start();

                user.Update += User_Update;

                Console.WriteLine($"User {PhoneNumber} started");
                StartedEvent?.Invoke(PhoneNumber);

                //timer = new System.Timers.Timer();
                //timer.Interval = 1000;
                //timer.AutoReset = true;
                //timer.Elapsed += Timer_Elapsed;
                //timer.Start();

                IsRunning = true;
            });
           
        }

        //private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{

        //    foreach (var (id, chat) in chats.chats)
        //        switch (chat)
        //        {

        //            case Channel channel when (channel.flags & Channel.Flags.broadcast) != 0:

        //                var msgs = await user.Messages_GetHistory(chat, limit: 10);

        //                var message = msgs.Messages[0];
        //                //foreach (var msg in msgs.Messages)
        //                //{
        //                //    Message m = (Message)msg;

        //                //    long groupId = 0;                            
        //                //    bool isFiltered = false;

        //                //    foreach (var word in FilteredWords)
        //                //    {
        //                //        if (m.message.Contains(word))
        //                //        {
        //                //            isFiltered = true;
        //                //        }
        //                //    }
        //                //}

        //                break;

        //            default:
        //                break;
        //        }

        //}

        public void Stop()
        {            
            timer?.Stop();
            user?.Dispose();            
            IsRunning = false; 
        }
        #endregion

        public event Action<string> NeedVerifyCodeEvent;
        public event Action<string> StartedEvent;

    }
}
