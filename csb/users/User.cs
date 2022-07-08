﻿using csb.bot_manager;
using csb.chains;
using csb.settings.validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace csb.users
{
    public class User
    {
        #region const
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
                    InlineKeyboardButton.WithCallbackData(text: "Добавить фильтр", callbackData: "addFilteredWord"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить фильтр", callbackData: "deleteFilteredWord"),
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

             {"saveFilteredWords", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Сохранить", callbackData: "saveFilteredWords"),
                }
            })},

        };
        #endregion

        #region vars          
        int currentChainID;
        BotState State;
        #endregion

        #region properties
        [JsonProperty]
        public long Id { get; set; }
        [JsonProperty]
        public string Name { get; set; }

        [JsonIgnore]
        public ITelegramBotClient bot { get; set; }
        [JsonIgnore]
        public MessagesProcessor messagesProcessor { get; set; }
        ChainProcessor chainprocessor;
        [JsonIgnore]
        public ChainProcessor chainsProcessor
        {
            get => chainprocessor;
            set
            {
                chainprocessor = value;
                chainprocessor.NeedVerifyCodeEvent += ChainsProcessor_NeedVerifyCodeEvent;
                chainprocessor.ChainStartedEvent += ChainProcessor_ChainStartedEvent;
            }
        }

        [JsonIgnore]
        public CancellationToken cancellationToken { get; set; }
        #endregion

        public User()
        {
        }

        #region private
        private async void ChainsProcessor_NeedVerifyCodeEvent(int id, string phone)
        {
            currentChainID = id;
            await sendTextMessage(Id, $"Введите код авторизации номера {phone}:");
            State = BotState.waitingVerificationCode;
        }
        private async void ChainProcessor_ChainStartedEvent(IChain chain)
        {
            await messagesProcessor.Back(Id);
            await sendTextMessage(Id, $"Цепочка {chain.ToString()} запущена");
            //await messagesProcessor.Delete(Id, "startsave");
            //await messagesProcessor.Delete(Id, "editChain");            
            //await showMyChains(Id);

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
                channel_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: channels[i].Item1, callbackData: $"channel_{channels[i].Item3}") };
            }

            channel_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(channel_buttons);

            return inlineKeyboard;
        }

        async Task<InlineKeyboardMarkup> getMyChannelsMarkUpGo(IChain chain)
        {

            var channels = await chain.User.GetAllChannels();
            int number = channels.Count;

            InlineKeyboardButton[][] channel_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                if (channels[i].Item2 != null)
                    channel_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(text: channels[i].Item1, url: $"http://t.me/{channels[i].Item2}") };
                else
                    channel_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: channels[i].Item1, callbackData: $"privateChannel_{channels[i].Item3}") };
            }

            channel_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(channel_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyBotsMarkUp(IChain chain)
        {
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

        InlineKeyboardMarkup getFilteredWordsMarkUp(IChain chain)
        {
            var words = chain.User.FilteredWords;
            int number = words.Count;

            InlineKeyboardButton[][] words_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                words_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: words[i], callbackData: $"filteredWords_{i}") };
            }
            words_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "× Очистить", callbackData: "clearFilteredWords") };
            words_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };            

            InlineKeyboardMarkup inlineKeyboard = new(words_buttons);

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
            var markup = await getMyChannelsMarkUpGo(chain);

            await messagesProcessor.Add(chat, "showInputChannels", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Входные каналы:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showDeleteBot(long chat, IChain chain)
        {
            var markup = getMyBotsMarkUp(chain);

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

        async Task showFilteredWords(long chat, IChain chain)
        {
            var markup = getFilteredWordsMarkUp(chain);

            await messagesProcessor.Add(chat, "showFilteredWords", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Фильтры для сообщений:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }
        #endregion

        #region public
        public async Task processMessage(Update update)
        {
            var chat = update.Message.Chat.Id;
            string msg = update.Message.Text;
            if (msg == null)
                return;

            switch (msg)
            {
                case "/start":
                    await sendTextMessage(chat, "Введите пароль...");
                    break;

                case "/mychains":
                    State = BotState.free;
                    try
                    {
                        await messagesProcessor.Clear(chat);
                        await showMyChains(chat);
                    } catch (Exception ex)
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
                                chain.AddBot(token);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Добавьте бота в администраторы выходного канала и перешлите сюда сообщение из этого канала:", "addChainCancel"));


                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingOutputChannelId;
                            break;

                        case BotState.waitingOutputChannelId:
                            var frwd = update.Message;
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.ChannelID = frwd.ForwardFromChat.Id;
                                bot.ChannelTitle = frwd.ForwardFromChat.Title;
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingOutputChannelLink", await sendTextButtonMessage(chat, "Введите телеграм аккаунт (в формате @name), на который будут заменяться телеграм акаунты во входящих сообщениях. Введите 0, если ссылки на аккаунты нужно удалять:", "addChainCancel"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingOutputChannelLink;
                            break;

                        case BotState.waitingOutputChannelLink:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.ChannelLink = msg;
                                chain.State = ChainState.X;
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                            }

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
                                await chain.User.AddInputChannel(msg.Trim());
                                await messagesProcessor.Delete(chat, "saveInputChannel");
                                await messagesProcessor.Add(chat, "saveInputChannels", await sendTextButtonMessage(chat, "Добавьте еще входной канал или нажмите \"Завершить\"", "saveInputChannels"));
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingFilteredWord:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.AddFilteredWord(msg);
                                await messagesProcessor.Delete(chat, "saveInputChannel");
                                await messagesProcessor.Add(chat, "saveFilteredWords", await sendTextButtonMessage(chat, "Добавьте еще фильтр или нажмите \"Сохранить\"", "saveFilteredWords"));

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
        public async Task processCallbackQuery(CallbackQuery query)
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
                                      $"Bots:\n" + botsinfo +
                                      $"IsActive={chain.IsRunning}\n";
                        await messagesProcessor.Add(chat, "back", await sendTextButtonMessage(chat, info, "back"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "saveAndStartChain":
                    try
                    {
                        chainsProcessor.Save();
                        chainsProcessor.Start(currentChainID);
                        //await messagesProcessor.Delete(chat, "startsave");
                        //await showMyChains(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Запуск...");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "startChain":
                    try
                    {
                        chainsProcessor.Start(currentChainID);
                        //await messagesProcessor.Back(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Запуск...");
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
                    } catch (Exception ex)
                    {
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


                case "addFilteredWord":
                    try
                    {                        
                        await messagesProcessor.Add(chat, "addFilteredWords", await sendTextButtonMessage(chat, "Введите текст. Сообщения, содержащие данный текст, будут отфильтрованы и не будут пересылаться в выходной канал:", "back"));
                        State = BotState.waitingFilteredWord;
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;
                    

                case "deleteFilteredWord":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        await showFilteredWords(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Выберите фильтр, который нужно удалить");

                    } catch (Exception ex)
                    {

                    }
                    
                    break;

                case "saveFilteredWords":
                    State = BotState.free;
                    chainsProcessor.Save();                    
                    await messagesProcessor.Back(chat);
                    await messagesProcessor.Delete(chat, "addFilteredWords");                    
                    await bot.AnswerCallbackQueryAsync(query.Id);
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
                            string id = data.Replace("channel_", "");
                            var chain = chainsProcessor.Get(currentChainID);
                            await chain.User.LeaveChannel(id);
                            await showDeleteChannel(chat, chain);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Канал удален");


                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }

                    }

                    if (data.Contains("privateChannel_"))
                    {
                        try
                        {
                            string id = data.Replace("privateChannel_", "");
                            await bot.AnswerCallbackQueryAsync(query.Id, $"Id={id}");


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

                    if (data.Contains("filteredWords_"))
                    {
                        try
                        {
                            string sindex = data.Replace("filteredWords_", "");
                            int index = int.Parse(sindex);
                            var chain = chainsProcessor.Get(currentChainID);
                            chain.RemoveFilteredWord(index);
                            chainprocessor.Save();
                            await showFilteredWords(chat, chain);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Фильтр удален");

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("clearFilteredWords"))
                    {
                        try
                        {
                            var chain = chainsProcessor.Get(currentChainID);
                            chain.ClearFilteredWords();
                            chainprocessor.Save();
                            await messagesProcessor.Back(chat);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Фильтры удалены");

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    break;
            }
        }
        #endregion
    }
}
