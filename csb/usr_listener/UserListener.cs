﻿using csb.bot_poster;
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
        Contacts_ResolvedPeer resolved = null;
        MediaGroup mediaGroup = new();

        private readonly ManualResetEventSlim codeReady = new();
        #endregion

        #region properties
        public string PhoneNumber { get; set; }
        public string CorrespondingBotName { get; set; }

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

                        //if (m.fwd_from != null)
                        //    continue;

                        if (resolved == null)
                            resolved = await user.Contacts_ResolveUsername(CorrespondingBotName);

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
                                await user.Messages_ForwardMessages(from_chat, new[] { unm.message.ID }, new[] { WTelegram.Helpers.RandomLong() }, resolved);
                                break;

                        }
                        break;                        
                }
            }            
        }

        private async void MediaGroup_MediaReadyEvent(MediaGroup group)
        {
            await user.Messages_ForwardMessages(from_chat, group.MessageIDs.ToArray(), group.MessageRands.ToArray(), resolved);
        }

        #region public
        public void SetVerifyCode(string code)
        {
            VerifyCode = code;
            codeReady.Set();
        }

        public async Task AddInputChannel(string input)
        {
            input = input.Replace("https://t.me/", "").Replace("+", "");

            var cci = await user.Messages_CheckChatInvite(input);
            //var ici = await user.Messages_ImportChatInvite(input);            
            //user.Channels_JoinChannel()

        }

        public void Start()
        {

            mediaGroup = new();
            mediaGroup.MediaReadyEvent += MediaGroup_MediaReadyEvent;

            Task.Run(async () => { 
                user = new Client(Config);                           
                var usr = await user.LoginUserIfNeeded();
                chats = await user.Messages_GetAllChats();
                dialogs = await user.Messages_GetAllDialogs();
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
