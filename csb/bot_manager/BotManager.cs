using csb.bot_poster;
using csb.chains;
using csb.settings;
using csb.settings.validators;
using csb.users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.bot_manager
{

    public enum BotState
    {
        free,
        waitingChainName,        
        waitingPhoneNumber,
        waitingToken,     
        waitingOutputChannelId,
        waitingOutputChannelLink,
        waitingVerificationCode,

        waitingAddInputChannel,       
    }
    
    public class MessagesProcessor
    {   

        #region vars
        Dictionary<string, Message> messages = new();
        ITelegramBotClient bot;
        #endregion

        public MessagesProcessor(ITelegramBotClient bot)
        {
            this.bot = bot;
        }

        public async Task Add(long chat, string key, Message message)
        {
            if (messages.ContainsKey(key))
            {
                var msg = messages[key];
                await bot.DeleteMessageAsync(chat, msg.MessageId);
                messages.Remove(key);
            }

                messages.Add(key, message);
        }

        public async Task Delete(long chat, string key)
        {
            if (messages.ContainsKey(key))
            {
                try
                {
                    await bot.DeleteMessageAsync(chat, messages[key].MessageId);
                    messages.Remove(key);
                } catch { }
            }
        }

        public async Task Clear(long chat)
        {
            foreach (var item in messages)
            {
                if (item.Value.Chat.Id == chat)
                    await bot.DeleteMessageAsync(chat, item.Value.MessageId);
            }
            messages.Clear();
        }

        public async Task Back(long chat)
        {
            try
            {
                var msg = messages.ElementAt(messages.Count - 1);
                await bot.DeleteMessageAsync(chat, msg.Value.MessageId);
                messages.Remove(msg.Key);
                //msg = messages.ElementAt(messages.Count - 1);
                //await bot.DeleteMessageAsync(chat, msg.Value.MessageId);
                //messages.Remove(msg.Key);
            } catch { }
        }
    }

    public class BotManager
    {

        #region vars
        ITelegramBotClient bot;
        CancellationToken cancellationToken;
        ChainProcessor chainsProcessor = new ChainProcessor("chains.json");        
        BotState State;

        UserManager userManager;

        int currentChainID;
        MessagesProcessor messagesProcessor;

        Dictionary<string, InlineKeyboardMarkup> inlineKeyboards = new Dictionary<string, InlineKeyboardMarkup>()
        {

            {"/mychains", new(new[] { 

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить цепочку", callbackData: "newchain"),
                }
            
            })},

            {"startsave", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Сохранить цепочку", callbackData: "saveChain"),                    
                },
                new[] {
                  InlineKeyboardButton.WithCallbackData(text: "Сохранить и запустить цепочку", callbackData: "saveAndStartChain"),
                }

            })},

            {"running_chain_actions", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "viewChain"),
                    InlineKeyboardButton.WithCallbackData(text: "Изменить", callbackData: "editChain"),                    
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Остановить", callbackData: "stopChain"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "deleteChain"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }

            })},

            {"idle_chain_actions", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "viewChain"),
                    InlineKeyboardButton.WithCallbackData(text: "Изменить", callbackData: "editChain"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Запустить", callbackData: "startChain"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "deleteChain"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }

            })},

            {"editChain", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить входной канал", callbackData: "addInputChannel"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить входной канал", callbackData: "deleteInputChannel"),
                    //InlineKeyboardButton.WithCallbackData(text: "Изменить", callbackData: "editChain"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить бота", callbackData: "addOutputBot"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить бота", callbackData: "deleteOutputBot"),
                    //InlineKeyboardButton.WithCallbackData(text: "Изменить", callbackData: "editChain"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                },
                //new[] {
                //    InlineKeyboardButton.WithCallbackData(text: "Запустить", callbackData: "startChain"),
                //    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "deleteChain"),
                //},
                //new[] {
                //    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                //}

            })},

            {"back", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

             {"addChainCancel", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "addChainCancel"),
                }
            })},

             {"saveInputChannels", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Завершить", callbackData: "saveInputChannels"),
                }
            })},

        };
        #endregion

        public BotManager()
        {

            userManager = new UserManager();
            userManager.Init();
#if DEBUG
            bot = new TelegramBotClient("5386081110:AAH71hl90ItlSNK7XSLguxUOC_e8gJNxRiQ");
#else
            bot = new TelegramBotClient("5597155386:AAEvPn9KUuWRPCECuOTJDHdh6RiY_IVbpWM");
#endif
            messagesProcessor = new MessagesProcessor(bot);
            chainsProcessor.NeedVerifyCodeEvent += ChainsProcessor_NeedVerifyCodeEvent;
            chainsProcessor.Load();
            chainsProcessor.StartAll();
            
        }

        private async void ChainsProcessor_NeedVerifyCodeEvent(int id, string phone)
        {
            currentChainID = id;

            foreach (var chat in userManager.GetIDs())
            {
                await sendTextMessage(chat, $"Введите код авторизации номера {phone}:");
            }
            State = BotState.waitingVerificationCode;
        }

        public async void Start()
        {
            cancellationToken = new CancellationToken();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery }
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, new CancellationToken());

            //foreach (var chat in userManager.GetIDs())
            //{
            //    await showMyChains(chat);
            //}
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {

            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    await processCallbackQuery(update.CallbackQuery);
                    break;

                    case UpdateType.Message:                    
                    await processMessage(update);
                    break;
            }            
        }

        InlineKeyboardMarkup getMyChainsMarkUp()
        {

            List<InlineKeyboardButton> chains = new();
            
            foreach (var item in chainsProcessor.Chains)
            {
                chains.Add(InlineKeyboardButton.WithCallbackData(text: item.ToString(), callbackData: $"chain_{item.Id}"));
            }

            InlineKeyboardMarkup inlineKeyboard = new(new[]

                    {
                        // first row
                        new []
                        {
                             InlineKeyboardButton.WithCallbackData(text: "Добавить цепочку", callbackData: "newchain"),
                            //InlineKeyboardButton.WithSwitchInlineQuery("switch_inline_query"),
                            //InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("switch_inline_query_current_chat"),                            
                        },

                        chains.ToArray()
                       
                    });
            return inlineKeyboard;
        }

        async Task<InlineKeyboardMarkup> getMyChannelsMarkUp(IChain chain)
        {

            var channels = await chain.User.GetAllChannels();
            int number = channels.Count;

            InlineKeyboardButton[][] channel_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                channel_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: channels[i].Item1, callbackData: $"channel_{channels[i].Item1}") };
            }

            channel_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(channel_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyBotsMarkUp(IChain chain)
        {

            //var channels = await chain.User.GetAllChannels();
            //int number = channels.Count;
            var bots = chain.Bots;
            int number = bots.Count;

            InlineKeyboardButton[][] bots_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                bots_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: bots[i].Name, callbackData: $"bot_{bots[i].Name}") };
            }
            bots_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(bots_buttons);

            return inlineKeyboard;
        }

        async Task<Message> sendTextMessage(long chat, string message)
        {
            return await bot.SendTextMessageAsync(
                chatId: chat,
                text: message,                
                cancellationToken: cancellationToken);
        }

        async Task<Message> sendTextButtonMessage(long chat, string message, string key)
        {
            return await bot.SendTextMessageAsync(
                chatId: chat,
                text: message,
                replyMarkup: inlineKeyboards[key],
                cancellationToken: cancellationToken);
        }

        async Task showMyChains(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mychains", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Управление цепочками:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyChainsMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task showDeleteChannel(long chat, IChain chain)
        {
            var markup = await getMyChannelsMarkUp(chain);

            await messagesProcessor.Add(chat, "deleteInputChannel", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Удалить канал:",                
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showListChannels(long chat, IChain chain)
        {
            var markup = await getMyChannelsMarkUp(chain);

            await messagesProcessor.Add(chat, "showInputChannels", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Входные каналы:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showDeleteBot(long chat, IChain chain)
        {
            var markup =  getMyBotsMarkUp(chain);

            await messagesProcessor.Add(chat, "deleteOutputBot", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите имя бота, которого необходимо удалить из цепочки:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showOutputBots(long chat, IChain chain)
        {
            var markup = getMyBotsMarkUp(chain);

            await messagesProcessor.Add(chat, "showOutputBots", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выходные боты:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        private async Task processMessage(Update update)
        {            
            var chat = update.Message.Chat.Id;
            string msg = update.Message.Text;
            if (msg == null)
                return;

            if (msg.Equals("5555"))
            {
                string name = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";
                userManager.Add(chat, name);
                //await bot.SendTextMessageAsync(chat, 
                //    "Вас приветствует бот Вдуть 2.0!\n" +
                //    "Бот позволяет копировать чужие каналы и заменять в них ссылки.\n" +
                //    "Для копирования канала нужно создать цепочку.\n" +
                //    "Цепочка состоит из пользователя-шпиона и выводного бота:\n" +
                //    "-Шпион должены быть подписан на чужой канал;\n" +
                //    "-Выводной бот должен быть администратором канала, в который будут копироваться данные.\n\n" +
                //    @"/mychains - список цепочек"
                //    );

                string helloText = "Вас приветствует бот Вдуть 2.0!\n" +
                    "Бот позволяет копировать чужие каналы (входные каналы) и заменять в них ссылки.\n" +
                    "Для копирования канала нужно создать цепочку.\n" +
                    "Цепочка состоит из пользователя-шпиона и выводных ботов:\n" +
                    "-Шпион должены быть подписан на чужой канал;\n" +
                    "-Выводной бот должен быть администратором одного канала, в который будут копироваться данные (выходной канал);\n" +
                    "-Выходных каналов может быть несколько.\n\n" +
                    @"/mychains - управление цепочками";

                await sendTextMessage(chat, helloText);
                //await showMyChains(chat);
                return;
            }

            if (!userManager.Check(chat) && !msg.Equals("/start"))
            {
                await sendTextMessage(chat, "Нет доступа");
                return;
            }

            switch (msg)
            {
                case "/start":
                    await sendTextMessage(chat, "Введите пароль...");
                    break;

                case "/mychains":
                    State = BotState.free;
                    //messagesProcessor.Add("/mychains", await bot.SendTextMessageAsync(
                    //    chatId: chat,
                    //    text: "Управление цепочками:",
                    //    //replyMarkup: inlineKeyboards["/mychains"],
                    //    replyMarkup: getMyChainsMarkUp(),
                    //    cancellationToken: cancellationToken));
                    try
                    {
                        await messagesProcessor.Clear(chat);
                        await showMyChains(chat);
                    } catch(Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
                    }
                    break;

                default:
                    switch (State)
                    {

                        case BotState.waitingChainName:
                            try
                            {
                                currentChainID = chainsProcessor.Add(msg);                                

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            //chainsProcessor.Save();
                            await messagesProcessor.Add(chat, "waitingPhoneNumber", await sendTextButtonMessage(chat, "Введите номер телефона аккаунта-шпиона:", "addChainCancel"));
                            //await sendTextMessage(chat, "Введите номер телефона аккаунта-шпиона:");
                            State = BotState.waitingPhoneNumber;
                            break;

                        case BotState.waitingPhoneNumber:
                            string phoneNumber = msg;
                            IValidator ph_vl = new PhoneNumberValidator();
                            if (!ph_vl.IsValid(phoneNumber))
                            {
                                await sendTextMessage(chat, ph_vl.Message);
                                return;
                            }
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.PhoneNumber = phoneNumber;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            string tokenMsg = $"Создайте выводного бота. Для этого выполните следующие действия:\n" +
                                               "1.Перейдите в @BotFather и там введите команду /newbot.\n" +
                                               "2.Придумайте и отправьте название выводного бота. Название должно быть понятным (например output_ch_1).\n" +
                                               "3.Далее отправьте username бота. Username бота должен иметь вид: названиеБота_bot (например output_ch_1_bot).\n" +                                               
                                               "4.@BotFather отправит вам сообщение с API-токеном. Скопируйте токен и отпавьте его сюда:";


                            await messagesProcessor.Add(chat, "waitingToken", await sendTextButtonMessage(chat, tokenMsg, "addChainCancel"));
                            //await sendTextMessage(chat, tokenMsg);
                            State = BotState.waitingToken;
                            break;

                        case BotState.waitingToken:
                            string token = msg;
                            IValidator token_vl = new TokenValidator();
                            if (!token_vl.IsValid(token))
                            {
                                await sendTextMessage(chat, token_vl.Message);
                                return;
                            }
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                //chain.Bots.Add(new BotPoster_api(token));
                                chain.AddBot(token);

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Добавьте бота в администраторы выходного канала и перешлите сюда сообщение из этого канала:", "addChainCancel"));
                            //await sendTextMessage(chat, "Добавьте бота в администраторы выходного канала и перешлите сюда сообщение из этого канала.");
                            State = BotState.waitingOutputChannelId;
                            //messagesProcessor.Add("startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));
                            break;

                        case BotState.waitingOutputChannelId:
                            var frwd = update.Message;                            
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.ChannelID = frwd.ForwardFromChat.Id;
                                bot.ChannelTitle = frwd.ForwardFromChat.Title;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            await messagesProcessor.Add(chat, "waitingOutputChannelLink", await sendTextButtonMessage(chat, "Введите телеграм аккаунт (в формате @name) ведущего канала, котороуму будут направляться сообщения подписчиков:", "addChainCancel"));
                            //await sendTextMessage(chat, "Введите телеграм аккаунт (в формате @name) ведущего канала, котороуму будут направляться сообщения подписчиков");
                            State = BotState.waitingOutputChannelLink;
                            //await messagesProcessor.Add(chat, "startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));
                            //State = BotState.free;
                            break;

                        case BotState.waitingOutputChannelLink:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.ChannelLink = msg;
                                chain.State = ChainState.X;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                            }
                            await messagesProcessor.Add(chat, "startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));
                            State = BotState.free;
                            break;

                        case BotState.waitingVerificationCode:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.SetVerifyCode(msg);
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAddInputChannel:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                await chain.User.AddInputChannel(msg);
                                await messagesProcessor.Delete(chat, "saveInputChannel");
                                await messagesProcessor.Add(chat, "saveInputChannels", await sendTextButtonMessage(chat, "Добавьте еще входной канал или нажмите \"Завершить\"", "saveInputChannels"));
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                    }
                    break;

            }
        }

        private async Task processCallbackQuery(CallbackQuery query)
        {

            long chat = query.Message.Chat.Id;

            switch (query.Data)
            {               

                case "newchain":
                    State = BotState.waitingChainName;
                    currentChainID = 0;
                    try
                    {
                        //await sendTextMessage(chat, "Введите имя новой цепочки:");
                        await messagesProcessor.Add(chat, "newchain", await sendTextButtonMessage(chat, "Введите имя новой цепочки", "addChainCancel"));                       
                        await messagesProcessor.Delete(chat, "/mychains");
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "saveChain":
                    try
                    {
                        chainsProcessor.Get(currentChainID);
                        chainsProcessor.Save();                        
                        await messagesProcessor.Delete(chat, "startsave");
                        await showMyChains(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Цепочка сохранена");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);                        
                    }                  
                    break;

                case "saveAndStartChain":
                    try
                    {
                        chainsProcessor.Save();
                        await chainsProcessor.Start(currentChainID);
                        await messagesProcessor.Delete(chat, "startsave");
                        await showMyChains(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "viewChain":
                    try
                    {
                        string botsinfo = "";                        
                        var chain = chainsProcessor.Get(currentChainID);
                        foreach (var bot in chain.Bots)
                        {
                            botsinfo += $"{bot.ToString()}\n";
                        }

                        string info = $"Id={chain.Id}\n" +
                                      $"Name={chain.Name}\n" +
                                      $"Phone={chain.PhoneNumber}\n" +
                                      $"Bots:\n" + botsinfo+
                                      //$"Token={chain.Token}\n" +w
                                      //$"OutputBotName={chain.OutputBotName}\n" +

                                      //$"OutputChId={chain.OutputChannelID}\n" +
                                      //$"OutputChLink={chain.OutputChannelLink}\n" +
                                      //$"OutputChName={chain.OutputChannelTitle}\n" +                                      
                                      $"IsActive={chain.IsRunning}\n";
                        await messagesProcessor.Add(chat, "back", await sendTextButtonMessage(chat, info, "back"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "startChain":    
                    try
                    {
                        await chainsProcessor.Start(currentChainID);
                        await messagesProcessor.Back(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Цепочка запущена");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "stopChain":
                    try
                    {
                        chainsProcessor.Stop(currentChainID);
                        await messagesProcessor.Back(chat);                        
                        await bot.AnswerCallbackQueryAsync(query.Id, "Цепочка остановлена");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "editChain":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);                       
                        await messagesProcessor.Add(chat, "editChain", await sendTextButtonMessage(chat, $"Редактирование цепочки {chain.ToString()}", "editChain"));
                        await bot.AnswerCallbackQueryAsync(query.Id, "Начато редактирование");                        

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addInputChannel":
                    try
                    {
                        await messagesProcessor.Add(chat, "addInputChannel", await sendTextButtonMessage(chat, "Введите ссылку на канала в формате @channel, t․me/joinchat/channel или t․me/+XYZxyz", "back"));
                        //await sendTextMessage(chat, "Введите ссылку на канала в формате @channel, t․me/joinchat/channel или t․me/+XYZxyz");
                        State = BotState.waitingAddInputChannel;
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "saveInputChannels":
                    try
                    {
                        State = BotState.free;
                        var chain = chainsProcessor.Get(currentChainID);
                        await chain.User.ResoreInputChannels();
                        await messagesProcessor.Back(chat);
                        await messagesProcessor.Delete(chat, "addInputChannel");
                        await showListChannels(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteInputChannel":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        await showDeleteChannel(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Выберите канал для удаления");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addOutputBot":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        chain.State = ChainState.edditing;

                        await messagesProcessor.Add(chat, "addOutputBot", await sendTextButtonMessage(chat, "Введите токен бота:", "addChainCancel"));
                        State = BotState.waitingToken;
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteOutputBot":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        await showDeleteBot(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Выберите");

                    } catch (Exception ex) 
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;                      

                case "deleteChain":
                    try
                    {                        
                        chainsProcessor.Delete(currentChainID);
                        await messagesProcessor.Back(chat);
                        await messagesProcessor.Back(chat);
                        await showMyChains(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Цепочка удалена");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "back":
                    try
                    {
                        State = BotState.free;
                        await messagesProcessor.Back(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex) {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addChainCancel":
                    try
                    {

                        switch (State)
                        {
                            case BotState.waitingChainName:
                                await messagesProcessor.Back(chat);
                                break;

                            case BotState.waitingPhoneNumber:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                break;

                            case BotState.waitingToken:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                break;

                            case BotState.waitingOutputChannelId:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                break;

                            case BotState.waitingOutputChannelLink:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                break;
                        }

                        State = BotState.free;

                        var chain = chainsProcessor.Get(currentChainID);
                        if (chain.State == ChainState.creating)
                            chainsProcessor.Delete(currentChainID);

                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {

                    }
                    break;

                case "":
                    break;

                default:

                    string data = query.Data;

                    if (data.Contains("chain_"))
                    {
                        currentChainID = int.Parse(data.Replace("chain_", ""));                       
                        
                        try
                        {
                            var chain = chainsProcessor.Get(currentChainID);

                            string m = $"Выбрана цепочка {chain.ToString()}. Что сделать?";

                            if (chain.IsRunning)
                            {
                                await messagesProcessor.Add(chat, "running_chain_actions", await sendTextButtonMessage(chat, m, "running_chain_actions"));
                            } else
                            {
                                await messagesProcessor.Add(chat, "idle_chain_actions", await sendTextButtonMessage(chat, m, "idle_chain_actions"));
                            }

                            await bot.AnswerCallbackQueryAsync(query.Id);

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }

                        //await bot.AnswerCallbackQueryAsync(query.Id, $"Выбрана цепочка Id={currentChainID}");
                    }

                    if (data.Contains("channel_"))
                    {
                        try
                        {
                            string title = data.Replace("channel_", "");
                            var chain = chainsProcessor.Get(currentChainID);
                            await chain.User.LeaveChannel(title);                            
                            await showDeleteChannel(chat, chain);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Канал удален");                            


                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }

                    }

                    if (data.Contains("bot_"))
                    {
                        try
                        {
                            string name = data.Replace("bot_", "");
                            var chain = chainsProcessor.Get(currentChainID);
                            chain.RemoveBot(name);
                            chainsProcessor.Save();
                            await showDeleteBot(chat, chain);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Бот удален");


                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    break;
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
