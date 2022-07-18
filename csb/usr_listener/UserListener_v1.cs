using csb.bot_poster;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace csb.usr_listener
{
    public class UserListener_v1
    {

        #region const
        const int messages_buffer_length = 20;
        #endregion

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


        List<(ChatBase, int)> nomediaIDs = new();
        List<(ChatBase, int)[]> mediaIDs = new();

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
        public string VerifyCode
        {
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

                    if (value < 1.0)
                        timeinterval = 0.05;

                    timer.Stop();
                    timer.Interval = 60 * timeinterval * 1000;
                    timer.Start();

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

        public UserListener_v1(string phonenumber)
        {
            PhoneNumber = phonenumber;

            timer = new();
            //timer.Interval = 60 * TimeInterval * 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        private void User_Update(TL.IObject u)
        {

            if (u is not UpdatesBase updates)
                return;

            Console.WriteLine($"Updates Length = {updates.UpdateList.Length}");

            foreach (var update in updates.UpdateList)
            {

                switch (update)
                {

                    case UpdateNewMessage unm:

                        var messageID = unm.message.ID;

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
                                        Console.WriteLine($"filtered by: {item}");
                                        return;
                                    }
                            }


                            from_chat = chats.chats[unm.message.Peer.ID];
                        } catch (Exception ex)
                        {
                            return;
                        }

#if RELEASE
                        if (m.fwd_from != null)
                            continue;
#endif


                        switch (m.media)
                        {

                            case MessageMediaPhoto mmp:
                                mediaGroup.Update(from_chat, m.grouped_id, unm.message.ID);
                                break;

                            case MessageMediaDocument mmd:
                                if (((Document)mmd.document).mime_type.Equals(""))
                                    return;
                                mediaGroup.Update(from_chat, m.grouped_id, unm.message.ID);
                                break;

                            default:


                                //foreach (var item in resolvedBots)
                                //{
                                //    try
                                //    {
                                //        long rand = Helpers.RandomLong();
                                //        await user.Messages_ForwardMessages(from_chat, new[] { messageID }, new[] { rand }, item);
                                //    } catch (Exception ex)
                                //    {
                                //        Console.WriteLine(ex.ToString());
                                //    }
                                //}

                                if (nomediaIDs.Count > messages_buffer_length)
                                    nomediaIDs.Clear();
                                nomediaIDs.Add((from_chat, unm.message.ID));
                                Console.WriteLine($"added text, total:{nomediaIDs.Count}");


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
                    Messages_MessagesBase message = await user.GetMessages(from_chat, group.MessageIDs[0].Item2);
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

                if (mediaIDs.Count > messages_buffer_length)
                    mediaIDs.Clear();
                mediaIDs.Add(group.MessageIDs.ToArray());
                Console.WriteLine($"added {group.MessageIDs.Count} medias, total:{mediaIDs.Count}");

                //foreach (var item in resolvedBots)
                //{
                //    try
                //    {
                //        List<long> rands = new();
                //        for (int i = 0; i < group.MessageRands.Count; i++)
                //            rands.Add(Helpers.RandomLong());
                //        //Суперважно менять рандомные айди при рассылке многим пользоватям одного и того же
                //        await user.Messages_ForwardMessages(from_chat, group.MessageIDs.ToArray(), /*group.MessageRands.ToArray()*/ rands.ToArray(), item);                        
                //    } catch (Exception ex)
                //    {
                //        Console.WriteLine(ex.ToString());
                //    }
                //}


            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } finally
            {
            }
        }

        async Task sendMedia()
        {

            if (mediaIDs.Count == 0)
            {
                Console.WriteLine("NO MORE MEDIA");
                return;
            }


            (ChatBase, int)[] ChatIDs = new(ChatBase, int)[mediaIDs[0].Length];
            for (int i = 0; i < ChatIDs.Length; i++)
                ChatIDs[i] = mediaIDs[0][i];
            mediaIDs.RemoveAt(0);

            Console.WriteLine($"{DateTime.Now} media length:{ChatIDs.Length}");

            foreach (var item in resolvedBots)
            {
                try
                {
                    List<long> rands = new();
                    for (int i = 0; i < ChatIDs.Length; i++)
                        rands.Add(Helpers.RandomLong());
                    //Суперважно менять рандомные айди при рассылке многим пользоватям одного и того же

                    int[] ids = new int[ChatIDs.Length];
                    for (int j = 0; j < ChatIDs.Length; j++)
                        ids[j] = ChatIDs[j].Item2;

                    await user.Messages_ForwardMessages(ChatIDs[0].Item1, ids, rands.ToArray(), item);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("RETRY MEDIA");
                    await sendMedia();
                }
            }

            Console.WriteLine($"{DateTime.Now} media sent");
        }

        async Task sendText()
        {

            if (nomediaIDs.Count == 0)
            {
                Console.WriteLine("NO MORE TEXT");
                return;
            }

            int id = nomediaIDs[0].Item2;
            ChatBase chat = nomediaIDs[0].Item1;
            nomediaIDs.RemoveAt(0);

            foreach (var item in resolvedBots)
            {
                try
                {
                    long rand = Helpers.RandomLong();
                    await user.Messages_ForwardMessages(chat, new[] { id }, new[] { rand }, item);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("RETRY TEXT");
                    await sendText();
                }
            }
        }


        Random rand = new Random();
        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            Console.WriteLine($"{DateTime.Now} {PhoneNumber}: timer elapsed");

            if (nomediaIDs.Count == 0 && mediaIDs.Count > 0)
            {                
                await sendMedia();                
                return;
            } 

            if (nomediaIDs.Count > 0 && mediaIDs.Count == 0)
            {                
                await sendText();             
                return;
            }

            if (nomediaIDs.Count > 0 && mediaIDs.Count > 0)
            {

                int r = rand.Next(100);
                Console.WriteLine($"r={r}");
                if (r < 20)
                {
                    Console.WriteLine($"sending checked text");
                    await sendText();
                    Console.WriteLine("sent checked text");
                } else
                {
                    Console.WriteLine($"sending checked media");
                    await sendMedia();
                    Console.WriteLine("sent checked media");
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
            } else
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
            List<(string, string, long)> res = new();
            //var chats = await user.Messages_GetAllChats();                

            if (chats == null)
                throw new Exception("Цепочка не была запущена, нет информации о входных каналах");

            await Task.Run(() =>
            {
                foreach (var item in chats.chats)
                {
                    if (item.Value is Channel channel)
                    {
                        //string name = (((Channel)item.Value).username != null) ? ((Channel)item.Value).username : ((Channel)item.Value).Title;

                        string title = ((Channel)item.Value).Title;
                        string username = ((Channel)item.Value).username;

                        res.Add(new(title, username, item.Value.ID));
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
            } catch (Exception ex)
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
