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
        waitingVerificationCode
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
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
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
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }

            })},

            {"back", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "back"),
                }
            })},

        };
        #endregion

        public BotManager()
        {

            userManager = new UserManager();
            userManager.Init();

            bot = new TelegramBotClient("5386081110:AAH71hl90ItlSNK7XSLguxUOC_e8gJNxRiQ");
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

            foreach (var chat in userManager.GetIDs())
            {
                await showMyChains(chat);
            }
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
                return;
            }

            if (!userManager.Check(chat))
            {
                await sendTextMessage(chat, "Нет доступа");
                return;
            }

            switch (msg)
            {   
                case "/mychains":
                    //State = BotState.free;
                    //messagesProcessor.Add("/mychains", await bot.SendTextMessageAsync(
                    //    chatId: chat,
                    //    text: "Управление цепочками:",
                    //    //replyMarkup: inlineKeyboards["/mychains"],
                    //    replyMarkup: getMyChainsMarkUp(),
                    //    cancellationToken: cancellationToken));
                    await showMyChains(chat);
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
                            await sendTextMessage(chat, "Введите номер телефона аккаунта-шпиона:");
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
                            await sendTextMessage(chat, "Введите токен бота:");
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
                                chain.Token = token;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            await sendTextMessage(chat, "Перешлите сюда сообщение из канала, в который будут направляться сообщения");
                            State = BotState.waitingOutputChannelId;
                            //messagesProcessor.Add("startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));
                            break;

                        case BotState.waitingOutputChannelId:
                            var frwd = update.Message;                            
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.OutputChannelID = frwd.ForwardFromChat.Id;
                                chain.OutputChannelTitle = frwd.ForwardFromChat.Title;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
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
                        await sendTextMessage(chat, "Введите имя новой цепочки:");
                        //messages.Add("newchain", await bot.SendTextMessageAsync(
                        //    chatId: query.Message.Chat.Id,
                        //    text: "Введите имя новой цепочки:",                        
                        //    cancellationToken: cancellationToken));
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
                        var chain = chainsProcessor.Get(currentChainID);
                        string info = $"Id={chain.Id}\n" +
                                      $"Name={chain.Name}\n" +
                                      $"Phone={chain.PhoneNumber}\n" +
                                      $"Token={chain.Token}\n" +
                                      $"OutputBotName={chain.OutputBotName}\n" +
                                      $"OutputChId={chain.OutputChannelID}\n" +
                                      $"OutputChName={chain.OutputChannelTitle}\n" +                                      
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
                        await messagesProcessor.Back(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex) {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
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
