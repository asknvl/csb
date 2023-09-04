﻿using asknvl.leads;
using csb.bot_manager;
using csb.bot_moderator;
using csb.bot_poster;
using csb.chains;
using csb.messaging;
using csb.moderation;
using csb.settings;
using csb.settings.validators;
using csb.usr_push;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

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
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить бота", callbackData: "addOutputBot"),
                    InlineKeyboardButton.WithCallbackData(text: "Обновить гео бота", callbackData: "editBotGeotag"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить бота", callbackData: "deleteOutputBot"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Установить период вывода сообщений", callbackData: "setMessagingPeriod"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить фильтр", callbackData: "addFilteredWord"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить фильтр", callbackData: "deleteFilteredWord"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить исключение", callbackData: "addReplacedWord"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить исключение", callbackData: "deleteReplacedWord"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить автозамену", callbackData: "addAutoChange"),
                    InlineKeyboardButton.WithCallbackData(text: "Удалить автозамену", callbackData: "deleteAutoChange"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настройка smart-уведомлений", callbackData: "editSmartPushes"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настройка ежедневных push-уведомлений", callbackData: "editDailyPushes"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настройка автоответов", callbackData: "editAutoAnswers"),
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

            {"editModerator", new(new[] {
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Изменить геотег", callbackData: "editModeratorGeoTag"),
                },

                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Алгоритм формирования лидов", callbackData: "checkLeadAlgorithm"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настроить Join сообщение", callbackData: "editJoinMessage"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настроить кружок", callbackData: "editPreJoinMessage"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настроить push-старт", callbackData: "editPushStart"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Настроить Leave сообщение", callbackData: "editLeaveMessage"),
                },

                //new[] {
                //    InlineKeyboardButton.WithCallbackData(text: "Настроить Push сообщения", callbackData: "editPushMessages"),
                //},

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить модератора", callbackData: "deleteModerator"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "backToMyModerators"),
                }
            })},

            {"editJoinMessageMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "greetings_join_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "greetings_join_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "greetings_join_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

            {"editPreJoinMessageMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "greetings_prejoin_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "greetings_prejoin_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "greetings_prejoin_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

            {"editLeaveMessageMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "greetings_leave_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "greetings_leave_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "greetings_leave_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

            {"editDailyPushesMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "daily_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "daily_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "daily_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

            {"editAutoAnswersMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "autoanswers_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "autoanswers_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "autoanswers_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

            {"finishDailyPushAdding", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Завершить", callbackData: "finishDailyPushAdding"),
                }
            })},

            {"finishDailyPushShow", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "finishDailyPushShow"),
                }
            })},

            {"editSmartPushesMenu", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "smart_show"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "smart_add"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "smart_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

             {"finishSmartPushAdding", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Завершить", callbackData: "finishSmartPushAdding"),
                }
            })},

            {"finishSmartPushShow", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "finishDailyPushShow"),
                }
            })},

            {"editAdmin", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Перезапустить админа", callbackData: "restartAdmin"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить админа", callbackData: "deleteAdmin"),
                },

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
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

             {"addModeratorCancel", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "addModeratorCancel"),
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

             {"saveReplacedWords", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Сохранить", callbackData: "saveReplacedWords"),
                }
            })},

             {"finishEddingGreetings", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Завершить", callbackData: "finishEddingGreetings"),
                }
            })},

              {"finishEddingJoinReply", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Завершить", callbackData: "finishEddingJoinReply"),
                }
            })},

             {"addAdminCancel", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "addAdminCancel"),
                }
            })},

             {"editPushMessage", new(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Посмотреть", callbackData: "push_message_show"),
                },                
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "push_message_delete"),
                },
                 new[] {
                    InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back"),
                }
            })},

             {"addSmartPushCancel", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "addSmartPushCancel"),
                }
            })},

             {"addpush", new(new[] {

                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Добавить", callbackData: "addpush"),
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData(text: "Отмена", callbackData: "back"),
                }
            })},

            

        };
        #endregion

        #region vars          
        int currentChainID;
        string currentBotGeoTag;
        string currentModeratorGeoTag;
        string currentAdminGeoTag;

        //PushMessage currentPushMessage;
        double currentPushMessageTime;


        string currentBotName;
        string oldText;
        BotState State;
        Queue<int> pushMessagesIds = new Queue<int>();
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

        //[JsonIgnore]
        //public ModerationProcessor moderationProcessor { get; set; }

        [JsonIgnore]
        public IModeratorsProcessor moderationProcessor { get; set; }

        TGUserManager<UserAdmin> adminmanager;
        [JsonIgnore]
        public TGUserManager<UserAdmin> adminManager {
            get => adminmanager;
            set
            {
                adminmanager = value;
                adminmanager.VerificationCodeRequestEvent += Adminmanager_VerificationCodeRequestEvent;
                adminmanager.UserStartedResultEvent += Adminmanager_UserStartedResultEvent;
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
        private async void Adminmanager_VerificationCodeRequestEvent(string geotag)
        {
            currentAdminGeoTag = geotag;
            await sendTextMessage(Id, $"Введите код авторизации для админиcтратора {geotag}:");
            State = BotState.waitingAdminVerificationCode;
        }

        private async void Adminmanager_UserStartedResultEvent(string geotag, bool res)
        {
            if (State == BotState.waitingAdminVerificationCode)
                State = BotState.free;
            if (res)
                await sendTextMessage(Id, $"Администратор {geotag} запущен");            
            else
                await sendTextMessage(Id, $"Администратор {geotag} не запущен");
        }
        InlineKeyboardMarkup getMyChainsMarkUp()
        {
            var chains = chainprocessor.Chains;
            int number = chains.Count;

            InlineKeyboardButton[][] chains_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                chains_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: chains[i].ToString(), callbackData: $"chain_{chains[i].Id}") };
            }

            chains_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Добавить цепочку", callbackData: "newchain") };

            InlineKeyboardMarkup inlineKeyboard = new(chains_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyModeratorsMarkUp()
        {

            var moderators = moderationProcessor.ModeratorBots.ToList();
            int number = moderators.Count;

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: moderators[i].GeoTag, callbackData: $"moderator_{moderators[i].GeoTag}") };
            }

            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Добавить модератора", callbackData: "newmoderator") };

            moderators_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };


            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyModeratorsDailyPushMessagesShowMarkUp()
        {

            //var moderators = moderationProcessor.ModeratorBots;
            //int number = moderators.Count;
            var bots = chainsProcessor.Get(currentChainID).Bots;
            var botsGeoTags = bots.Select(b => b.GeoTag);

            var moderators = moderationProcessor.ModeratorBots.Where(m => botsGeoTags.Contains(m.GeoTag)).ToList();
            int number = moderators.Count; //select for chain bots

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: moderators[i].GeoTag, callbackData: $"moderators_show_daily_pushes_{moderators[i].GeoTag}") };
            }

            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyModeratorsSmartPushMessagesShowMarkUp()
        {

            //var moderators = moderationProcessor.ModeratorBots;
            //int number = moderators.Count;
            var bots = chainsProcessor.Get(currentChainID).Bots;
            var botsGeoTags = bots.Select(b => b.GeoTag);

            var moderators = moderationProcessor.ModeratorBots.Where(m => botsGeoTags.Contains(m.GeoTag)).ToList();
            int number = moderators.Count; //select for chain bots

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: moderators[i].GeoTag, callbackData: $"moderators_show_smart_pushes_{moderators[i].GeoTag}") };
            }

            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyModeratorsDailyPushMessagesDeleteMarkUp()
        {

            var bots = chainsProcessor.Get(currentChainID).Bots;
            var botsGeoTags = bots.Select(b => b.GeoTag);

            var moderators = moderationProcessor.ModeratorBots.Where(m => botsGeoTags.Contains(m.GeoTag)).ToList();
            int number = moderators.Count; //select for chain bots

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: moderators[i].GeoTag, callbackData: $"moderators_delete_daily_pushes_{moderators[i].GeoTag}") };
            }
            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Для всех", callbackData: "moderators_delete_daily_pushes_all") };

            moderators_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyModeratorsSmartPushMessagesDeleteMarkUp()
        {

            var bots = chainsProcessor.Get(currentChainID).Bots;
            var botsGeoTags = bots.Select(b => b.GeoTag);

            var moderators = moderationProcessor.ModeratorBots.Where(m => botsGeoTags.Contains(m.GeoTag)).ToList();
            int number = moderators.Count; //select for chain bots

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: moderators[i].GeoTag, callbackData: $"moderators_delete_smart_pushes_{moderators[i].GeoTag}") };
            }
            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Для всех", callbackData: "moderators_delete_smart_pushes_all") };

            moderators_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

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

        InlineKeyboardMarkup getMyBotsDeleteMarkUp(IChain chain)
        {
            var bots = chain.Bots;
            int number = bots.Count;

            InlineKeyboardButton[][] bots_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                bots_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{bots[i].GeoTag} {bots[i].Name}", callbackData: $"bot_delete_{bots[i].Name}") };
            }
            bots_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(bots_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyBotsGeoTagMarkUp(IChain chain)
        {
            var bots = chain.Bots;
            int number = bots.Count;

            InlineKeyboardButton[][] bots_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                bots_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{bots[i].GeoTag} {bots[i].Name}", callbackData: $"bot_geotag_{bots[i].Name}") };
            }
            bots_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(bots_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyBotsWithActionMarkUp(IChain chain, string action)
        {
            var bots = chain.Bots;
            int number = bots.Count;

            InlineKeyboardButton[][] bots_buttons = new InlineKeyboardButton[number + 1][];

            for (int i = 0; i < number; i++)
            {
                bots_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{bots[i].GeoTag} {bots[i].Name}", callbackData: $"bot_{action}_{bots[i].Name}") };
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

        InlineKeyboardMarkup getReplacedWordsMarkUp(IChain chain)
        {
            var words = chain.ReplacedWords;
            int number = words.Count;

            InlineKeyboardButton[][] words_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                words_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: words[i], callbackData: $"replacedWords_{i}") };
            }
            words_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "× Очистить", callbackData: "clearReplacedWords") };
            words_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(words_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyChannelAdminsMarkUp()
        {

            var admins = adminManager.Users.ToArray();
            int number = admins.Length;

            InlineKeyboardButton[][] moderators_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                moderators_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: admins[i].geotag, callbackData: $"admin_{admins[i].geotag}") };
            }

            moderators_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Добавить администратора", callbackData: "newadmin") };
            moderators_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };

            InlineKeyboardMarkup inlineKeyboard = new(moderators_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getAdminMarkUp(UserAdmin admin)
        {
            InlineKeyboardButton[][] admins_buttons = new InlineKeyboardButton[3 + 1][];

            if (admin.IsRunning)
                admins_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Остановить администратора", callbackData: $"stopAdmin") };
            else
                admins_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Запустить администратора", callbackData: $"restartAdmin") };

            if (admin.NeedAutoAnswer)
                admins_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Автоответ ✓", callbackData: $"admin_{admin.geotag}_autoanswer_off") };
            else
                admins_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Автоответ", callbackData: $"admin_{admin.geotag}_autoanswer_on") };

            admins_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Удалить администратора", callbackData: $"deleteAdmin") };
            admins_buttons[3] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "back") };
            return new(admins_buttons);
        }

        InlineKeyboardMarkup getMyPushMessagesMarkUp()
        {

            var pushes = moderationProcessor.SmartPushData(currentModeratorGeoTag).Messages;
            int number = pushes.Count;

            InlineKeyboardButton[][] push_buttons = new InlineKeyboardButton[number + 2][];

            for (int i = 0; i < number; i++)
            {
                push_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{pushes[i].TimePeriod} часовое", callbackData: $"push_{pushes[i].TimePeriod}") };
            }

            push_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "Добавить push сообщение", callbackData: "addpush") };
            push_buttons[number + 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "backToModeratorShow") };

            InlineKeyboardMarkup inlineKeyboard = new(push_buttons);

            return inlineKeyboard;
        }

        InlineKeyboardMarkup getMyLeadAlgorithmsMarkUp(LeadAlgorithmType? type)
        {
            var algorithms = Enum.GetValues(typeof(LeadAlgorithmType));           
            int number = algorithms.Length;

            InlineKeyboardButton[][] type_buttons = new InlineKeyboardButton[number + 1][];
            int i = 0;

            foreach (var item in algorithms)
            {
                var text = ((LeadAlgorithmType)item).ToString();
                if (text.Equals(type.ToString()))
                    text = $"{text} ✓";
                type_buttons[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"{text}", callbackData: $"lead_algo_{(int)item}") };
                i++;
            }
            type_buttons[number] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "backToModeratorShow") };

            InlineKeyboardMarkup inlineKeyboard = new(type_buttons);

            return inlineKeyboard;

        }

        

        InlineKeyboardMarkup getMyPushStartMarkUp(bool usePushStart)
        {
            InlineKeyboardButton[][] pushstart_buttons = new InlineKeyboardButton[6][];
            int i = 0;

            if (usePushStart)
            {
                pushstart_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Использовать push-старт кнопку ✓", callbackData: $"pushstart_on") };
                pushstart_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Не использовать push-старт кнопку", callbackData: $"pushstart_off") };                
            }
            else
            {
                pushstart_buttons[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Использовать push-старт кнопку", callbackData: $"pushstart_on") };
                pushstart_buttons[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Не использовать push-старт кнопку ✓", callbackData: $"pushstart_off") };
            }

            pushstart_buttons[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Добавить ответ на push-start", callbackData: $"pushstart_add_reply") };
            pushstart_buttons[3] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Удалить ответ на push-start", callbackData: $"pushstart_remove_reply") };
            pushstart_buttons[4] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: $"Посмотреть ответ на push-start", callbackData: $"pushstart_show_reply") };

            pushstart_buttons[5] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(text: "« Назад", callbackData: "backToModeratorShow") };

            InlineKeyboardMarkup inlineKeyboard = new(pushstart_buttons);

            return inlineKeyboard;

        }

        async Task<Message> sendTextMessage(long chat, string message)
        {
            return await bot.SendTextMessageAsync(
                chatId: chat,
                text: message,
                disableWebPagePreview:true,
                cancellationToken: cancellationToken);
        }

        async Task<Message> sendTextButtonMessage(long chat, string message, string key)
        {
            return await bot.SendTextMessageAsync(
                chatId: chat,
                text: message,
                replyMarkup: inlineKeyboards[key],
                disableWebPagePreview:true,
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

        async Task showMyModerators(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mymoderators", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Управление модераторами:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyModeratorsMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task showMyModeratorsDailyPushesMessages(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mymoderators", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите модератора, для которого требуется показать ежедневные сообщения:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyModeratorsDailyPushMessagesShowMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task showMyModeratorsSmartPushesMessages(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mymoderators", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите модератора, для которого требуется показать smart-сообщения:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyModeratorsSmartPushMessagesShowMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task deleteMyModeratorsDailyPushMessages(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mymoderators", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите модератора, для которого требуется удалить ежедневные сообщения:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyModeratorsDailyPushMessagesDeleteMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task deleteMyModeratorsSmartPushMessages(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/mymoderators", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите модератора, для которого требуется удалить smart сообщения:",
                //replyMarkup: inlineKeyboards["/mychains"],
                replyMarkup: getMyModeratorsSmartPushMessagesDeleteMarkUp(),
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
            var markup = getMyBotsDeleteMarkUp(chain);

            await messagesProcessor.Add(chat, "deleteOutputBot", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите имя бота, которого необходимо удалить из цепочки:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showEditGeoTagBots(long chat, IChain chain)
        {
            var markup = getMyBotsGeoTagMarkUp(chain);

            await messagesProcessor.Add(chat, "showEditGeoTagBots", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите бота, геотег которого нужно обновить:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showAddAutochangeBots(long chat, IChain chain)
        {
            var markup = getMyBotsWithActionMarkUp(chain, "+autochange");

            await messagesProcessor.Add(chat, "showAddAutochangeBots", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите бота, для которого неободимо добавить автозамены:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showDeleteAutochangeBots(long chat, IChain chain)
        {
            var markup = getMyBotsWithActionMarkUp(chain, "-autochange");

            await messagesProcessor.Add(chat, "showDeleteAutochangeBots", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Выберите бота, для которого неободимо удалить автозамены:",
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

        async Task showReplacedWords(long chat, IChain chain)
        {
            var markup = getReplacedWordsMarkUp(chain);

            await messagesProcessor.Add(chat, "showReplacedWords", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Исключения:",
                replyMarkup: markup,
                cancellationToken: cancellationToken));
        }

        async Task showMyAdmins(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "/myadmins", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Управление администраторами:",
                replyMarkup: getMyChannelAdminsMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task showMyPushMessages(long chat)
        {
            State = BotState.free;
            await messagesProcessor.Add(chat, "showMyPushMessages", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Управление push-сообщениями:",
                replyMarkup: getMyPushMessagesMarkUp(),
                cancellationToken: cancellationToken));
        }

        async Task showMyModeratorsLeadType(long chat, LeadAlgorithmType? type)
        {
            await messagesProcessor.Add(chat, "checkLeadAlgorithm", await bot.SendTextMessageAsync(
                chatId: chat,
                text:"Выберите алгоритм для обработки лидов:",
                replyMarkup: getMyLeadAlgorithmsMarkUp(type),
                cancellationToken: cancellationToken));
        }

        async Task showMyModeratorsPushStart(long chat, bool usePushStart)
        {
            await messagesProcessor.Add(chat, "editPushStart", await bot.SendTextMessageAsync(
                chatId: chat,
                text: "Настройки push-start:",
                replyMarkup: getMyPushStartMarkUp(usePushStart),
                cancellationToken: cancellationToken));
        }

        async Task showAdminEdit(long chat, UserAdmin admin)
        {
            await messagesProcessor.Add(chat, "editAdmin", await bot.SendTextMessageAsync(
                chatId: chat,
                text: $"Выбран администратор {admin.geotag}. Что сделать?",
                replyMarkup: getAdminMarkUp(admin),
                cancellationToken: cancellationToken));
        }

        async Task showDeleteAutoChanges(long chat, string botname)
        {

            var autochanges = chainprocessor.Get(currentChainID).GetAutoChanges(botname);

            string ach = (autochanges.Count > 0) ? $"Введите номер автозамены для {botname}, которую требуется удалить (введите 0, чтобы удалить все):\n" : "Список автозамен пуст";

            int cntr = 0;
            foreach (var autochange in autochanges)
            {
                cntr++;
                ach += $"{cntr}\n\t{autochange.OldText} → {autochange.NewText}\n";
            }

            await messagesProcessor.Add(chat, "deleteAutoChange", await sendTextButtonMessage(chat, ach, "back"));
        }

        async Task showAutoChanges(long chat, string botname)
        {

            var autochanges = chainprocessor.Get(currentChainID).GetAutoChanges(botname);

            if (autochanges.Count == 0)
                return;

            string ach = $"Текущие автозамены бота:\n";

            int cntr = 0;
            foreach (var autochange in autochanges)
            {
                cntr++;
                ach += $"{cntr}\n\t{autochange.OldText} → {autochange.NewText}\n";
            }

            await messagesProcessor.Add(chat, "showAutoChange", await sendTextMessage(chat, ach));
        }
        #endregion

        #region public
        public async Task processMessage(Update update)
        {

            var chat = update.Message.Chat.Id;

            string msg = (update.Message.Text != null) ? update.Message.Text : "";
            //if (msg == null)
            //    return;

            if (msg.Contains("/setbufferlength"))
            {
                try
                {
                    string val = msg.Replace("/setbufferlength", "").Trim();
                    int len = int.Parse(val);
                    var chain = chainsProcessor.Get(currentChainID);
                    chain.User.SetMessageBufferLength(len);
                    chainsProcessor.Save();

                } catch (Exception ex)
                {
                    await sendTextMessage(chat, ex.Message);
                }
            }

            if (msg.Contains("/settreshold"))
            {
                try
                {
                    string val = msg.Replace("/settreshold", "").Trim();
                    int t = int.Parse(val);
                    var chain = chainsProcessor.Get(currentChainID);
                    chain.User.SetMatchingTreshold(t);
                    chainsProcessor.Save();

                } catch (Exception ex)
                {
                    await sendTextMessage(chat, ex.Message);
                }
            }

            if (msg.Contains("/set_pseudo_leads"))
            {
                try
                {
                    var splt = msg.Split(":");
                    string geotag = splt[1];
                    string pixel = splt[2];
                    int val = int.Parse(splt[3]);

                    var moderator = moderationProcessor.Get(geotag);
                    if (moderator != null)
                    {
                        moderator.PseudoLeads.Add(pixel, val);
                    }

                } catch (Exception ex)
                {
                    await sendTextMessage(chat, ex.Message);
                }
            }

            if (msg.Contains("/get_pseudo_leads"))
            {
                try
                {
                    var splt = msg.Split(":");
                    string geotag = splt[1];                    

                    var moderator = moderationProcessor.Get(geotag);
                    if (moderator != null)
                    {
                        string s = "";
                        foreach (var item in moderator.PseudoLeads)
                        {
                            s += $"{item.Key} {item.Value}";
                        }

                        string output = (!s.Equals("")) ? $"{geotag}:PseudeLeads={s}" : "No pseudo leads set";

                        await sendTextMessage(chat, output);
                    }

                }
                catch (Exception ex)
                {
                    await sendTextMessage(chat, ex.Message);
                }
            }

            if (msg.Contains("/del_pseudo_leads"))
            {
                try
                {
                    var splt = msg.Split(":");
                    string geotag = splt[1];
                    string pixel = splt[2];

                    var moderator = moderationProcessor.Get(geotag);
                    if (moderator != null)
                    {
                        if (moderator.PseudoLeads.ContainsKey(pixel))
                            moderator.PseudoLeads.Remove(pixel);
                    }

                }
                catch (Exception ex)
                {
                    await sendTextMessage(chat, ex.Message);
                }
            }

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

                case "/mymoderators":
                    State = BotState.free;
                    try
                    {
                        await messagesProcessor.Clear(chat);
                        await showMyModerators(chat);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
                    }
                    break;

                case "/moderatorslist":
                    try
                    {
                        var moderators = moderationProcessor.ModeratorBots;
                        string res = "";
                        foreach (var item in moderators)
                        {
                            res += $"{item.GeoTag} {item.Name}\n";
                        }
                        await sendTextMessage(chat, res);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
                    }
                    break;

                case "/adminslist":
                    try
                    {
                        var admins = adminManager.Users.ToArray();
                        string res = "";
                        foreach (var item in admins)
                        {
                            res += $"{item.geotag} {item.username} {item.phone_number}\n";
                        }
                        await sendTextMessage(chat, res);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
                    }
                    break;

                case "/myadmins":
                    State = BotState.free;
                    try
                    {
                        await messagesProcessor.Clear(chat);
                        await showMyAdmins(chat);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
                    }
                    break;

                case "/getbufferlength":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        int len = chain.User.GetMessageBufferLength();
                        await sendTextMessage(chat, "BufferLength=" + len);
                    } catch { }
                    break;

                case "/gettreshold":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        int t = chain.User.GetMatchingTreshold();
                        await sendTextMessage(chat, "MatchingTreshold=" + t);
                    } catch { }
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

                            //string tokenMsg = $"Создайте выводного бота. Для этого выполните следующие действия:\n" +
                            //                   "1.Перейдите в @BotFather и там введите команду /newbot.\n" +
                            //                   "2.Придумайте и отправьте название выводного бота. Название должно быть понятным (например output_ch_1).\n" +
                            //                   "3.Далее отправьте username бота. Username бота должен иметь вид: названиеБота_bot (например output_ch_1_bot).\n" +
                            //                   "4.@BotFather отправит вам сообщение с API-токеном. Скопируйте токен и отпавьте его сюда:";


                            //await messagesProcessor.Add(chat, "waitingToken", await sendTextButtonMessage(chat, tokenMsg, "addChainCancel"));
                            //State = BotState.waitingToken;
                            try
                            {
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Введите геотег бота:", "addChainCancel"));
                                State = BotState.waitingBotGeoTag;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            break;

                        case BotState.waitingBotGeoTag:
                            try
                            {

                                currentBotGeoTag = msg;

                                //var chain = chainsProcessor.Get(currentChainID);
                                //var bot = chain.Bots.Last();
                                //bot.GeoTag = msg;
                                //await messagesProcessor.Back(chat);
                                //await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Добавьте бота в администраторы выходного канала и перешлите сюда сообщение из этого канала:", "addChainCancel"));

                                string tokenMsg = $"Создайте выводного бота. Для этого выполните следующие действия:\n" +
                                                   "1.Перейдите в @BotFather и там введите команду /newbot.\n" +
                                                   "2.Придумайте и отправьте название выводного бота. Название должно быть понятным (например output_ch_1).\n" +
                                                   "3.Далее отправьте username бота. Username бота должен иметь вид: названиеБота_bot (например output_ch_1_bot).\n" +
                                                   "4.@BotFather отправит вам сообщение с API-токеном. Скопируйте токен и отпавьте его сюда:";


                                await messagesProcessor.Add(chat, "waitingToken", await sendTextButtonMessage(chat, tokenMsg, "addChainCancel"));
                                State = BotState.waitingToken;


                            }
                            catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
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
                                await chain.AddBot(token, currentBotGeoTag);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Добавьте бота в администраторы выходного канала и перешлите сюда сообщение из этого канала:", "addChainCancel"));

                            }
                            catch (Exception ex)
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
                                await messagesProcessor.Add(chat, "waitingOutputVictimLink", await sendTextButtonMessage(chat, "Введите телеграм аккаунт (в формате @name), КОТОРЫЙ требуется заменить во входящих сообщениях. Введите 0, если требуется заменять все аккаунты:", "addChainCancel"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingVictimChannelLink;
                            break;

                        case BotState.waitingVictimChannelLink:
                            try
                            {
                                IValidator tl_vl = new TelegramValidator();
                                if (!tl_vl.IsValid(msg))
                                {
                                    await sendTextMessage(chat, tl_vl.Message);
                                    return;
                                }

                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.VictimLink = msg;
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingOutputChannelLink", await sendTextButtonMessage(chat, "Введите телеграм аккаунт (в формате @name), НА КОТОРЫЙ будут заменяться телеграм акаунты во входящих сообщениях. Введите 0, если ссылки на аккаунты нужно удалять:", "addChainCancel"));
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
                                IValidator tl_vl = new TelegramValidator();
                                if (!tl_vl.IsValid(msg))
                                {
                                    await sendTextMessage(chat, tl_vl.Message);
                                    return;
                                }

                                var chain = chainsProcessor.Get(currentChainID);
                                var bot = chain.Bots.Last();
                                bot.ChannelLink = msg;

                                var autoChange = new AutoChange() { OldText = bot.VictimLink, NewText = bot.ChannelLink };
                                bot.AutoChanges.Add(autoChange);
                                chainprocessor.Save();                                

                                chain.State = ChainState.X;
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "startsave", await sendTextButtonMessage(chat, "Регистрация завершена. Выберите действие:", "startsave"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                            }

                            State = BotState.free;
                            break;

                        case BotState.waitingBotGeoTagEdit:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.EditBotGeotag(currentBotName, msg);
                                chainsProcessor.Save();
                                await showEditGeoTagBots(chat, chain);
                            }
                            catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                            }
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
                                var chain = chainsProcessor.Get(currentChainID);
                                return;
                            }
                            break;

                        case BotState.waitingReplacedWord:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.AddReplacedWord(msg);
                                await messagesProcessor.Delete(chat, "saveInputChannel");
                                await messagesProcessor.Add(chat, "saveReplacedWords", await sendTextButtonMessage(chat, "Добавьте еще удаляемую фразу или нажмите \"Сохранить\"", "saveReplacedWords"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                var chain = chainsProcessor.Get(currentChainID);
                                return;
                            }
                            break;

                        case BotState.waitingMessagingPeriod:
                            try
                            {
                                double period = double.Parse(msg.Trim());
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.SetMessagingPeriod(period);
                                chainprocessor.Save();
                                await messagesProcessor.Delete(chat, "setMessagingPeriod");
                                State = BotState.free;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorGeoTag:
                            try
                            {
                                string geotag = msg.Trim();
                                var found = moderationProcessor.ModeratorBots.FirstOrDefault(o => o.GeoTag.Equals(geotag));
                                if (found != null)
                                {
                                    throw new Exception("Бот с таким геотегом уже существует. Введите другой геотег:");
                                }

                                currentModeratorGeoTag = geotag;

                                string moderatorTokenMsg = $"Создайте бота-модератора. Для этого выполните следующие действия:\n" +
                                               "1.Перейдите в @BotFather и там введите команду /newbot.\n" +
                                               "2.Придумайте и отправьте название выводного бота. Название должно соответствоать ГЕО связки (например MODERATOR_IND1).\n" +
                                               "3.Далее отправьте username бота. Username бота должен иметь вид: названиеБота_bot (например MODERATOR_IND1_bot).\n" +
                                               "4.Скопируйте API-токен бота-модератора из @BotFather и отпавьте его сюда:\n" +
                                               "5.Добавьте бота в модерируемый канал.";
                                               

                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingModeratorToken", await sendTextButtonMessage(chat, moderatorTokenMsg, "addModeratorCancel"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingModeratorToken;
                            break;

                        case BotState.waitingModeratorToken:
                            string moder_token = msg;
                            IValidator moder_token_vl = new TokenValidator();
                            if (!moder_token_vl.IsValid(moder_token))
                            {
                                await sendTextMessage(chat, moder_token_vl.Message);
                                return;
                            }
                            try
                            {
                                IChain chain = null;
                                //string victimLink;
                                //string channelLink;

                                try
                                {
                                    chain = chainsProcessor.Get(currentModeratorGeoTag);
                                } catch (Exception ex)
                                {

                                }
                                if (chain != null)
                                {
                                    var outbot = chain.Bots.FirstOrDefault(b => b.GeoTag.Equals(currentModeratorGeoTag));
                                    //if (outbot != null)
                                    //{
                                    //    victimLink = outbot.VictimLink;
                                    //    channelLink = outbot.ChannelLink;
                                    //}

                                    //AutoChange pmAutochange = new AutoChange()
                                    //{
                                    //    OldText = outbot.VictimLink,
                                    //    NewText = outbot.ChannelLink
                                    //};

                                    List<AutoChange> allAutoChanges = null;
                                    if (outbot != null)
                                        allAutoChanges = new List<AutoChange>(outbot.AutoChanges);
                                    //allAutoChanges.Add(pmAutochange);

                                    //moderationProcessor.Add(moder_token, currentModeratorGeoTag, chain.DailyPushData, new List<AutoChange> { pmAutochange });
                                    moderationProcessor.Add(moder_token, currentModeratorGeoTag, chain.DailyPushData, chain.PushData, allAutoChanges);

                                } else
                                    moderationProcessor.Add(moder_token, currentModeratorGeoTag);

                                //moderationProcessor.Start(currentModeratorGeoTag);
                                await messagesProcessor.Back(chat);
                                await sendTextMessage(Id, $"Модератор {currentModeratorGeoTag} запущен");

                                string helloReqMsg = "Перешлите (forward) сюда приветственное \U0001F4B0 сообщение, если оно не требуется нажмите Завершить";
                                await messagesProcessor.Add(chat, "waitingModeratorHelloMessage", await sendTextButtonMessage(chat, helloReqMsg, "finishEddingGreetings"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingModeratorHelloMessage;
                            break;

                        case BotState.waitingModeratorHelloMessage:
                            try
                            {

                                JoinMessage join = await JoinMessage.Create(chat, bot, update.Message, currentModeratorGeoTag);
                                moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage = join;

                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.Text = update.Message.Text;
                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.Entities = update.Message.Entities;
                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.ReplyMarkup = update.Message.ReplyMarkup;


                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);

                                await messagesProcessor.Add(chat, "waitingModeratorByeMessage", await sendTextMessage(chat, "Приветственное сообщение создано"));

                                string helloReqMsg = "Перешлите (forward) сюда прощальное \U000026B0 сообщение, если оно не требуется нажмите Завершить";
                                await messagesProcessor.Add(chat, "waitingModeratorByeMessage", await sendTextButtonMessage(chat, helloReqMsg, "finishEddingGreetings"));

                                State = BotState.waitingModeratorByeMessage;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorHelloMessageEdit:
                            try
                            {
                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.Text = update.Message.Text;
                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.Entities = update.Message.Entities;
                                //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage.ReplyMarkup = update.Message.ReplyMarkup;
                                JoinMessage join = await JoinMessage.Create(chat, bot, update.Message, currentModeratorGeoTag);
                                moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage = join;


                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorByeMessage:
                            try
                            {
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.Text = update.Message.Text;
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.Entities = update.Message.Entities;
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.ReplyMarkup = update.Message.ReplyMarkup;

                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);


                                var chain = chainsProcessor.Get(currentModeratorGeoTag);
                                currentChainID = chain.Id;
                                await messagesProcessor.Add(chat, "editChain", await sendTextButtonMessage(chat, $"Редактирование цепочки {chain.ToString()}", "editChain"));

                                //await messagesProcessor.Add(chat, "addpush", await sendTextButtonMessage(chat, "Прощальное сообщение создано. Добавить часовые уведомления", "addpush"));
                                State = BotState.free;
                            }
                            catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorByeMessageEdit:
                            try
                            {
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.Text = update.Message.Text;
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.Entities = update.Message.Entities;
                                moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage.ReplyMarkup = update.Message.ReplyMarkup;

                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);

                                State = BotState.free;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorPreJoinMessageEdit:
                            try
                            {

                                JoinMessage prejoin = await JoinMessage.Create(chat, bot, update.Message, currentModeratorGeoTag);
                                moderationProcessor.Greetings(currentModeratorGeoTag).PreJoinMessage = prejoin;
                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorJoinMessageReplyEdit:
                            try
                            {
                                moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessageReply.Text = update.Message.Text;
                                moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessageReply.Entities = update.Message.Entities;
                                moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessageReply.ReplyMarkup = update.Message.ReplyMarkup;

                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);

                                State = BotState.free;
                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingModeratorGeoTagEdit:
                            try
                            {
                                var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                                moderator.GeoTag = msg;
                                moderationProcessor.Save();
                                await messagesProcessor.Back(chat);
                                await showMyModerators(chat);
                                State = BotState.free;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAdminGeoTag:
                            try
                            {
                                string geotag = msg.Trim();
                                var found = adminManager.Get(geotag);
                                if (found != null)
                                {
                                    throw new Exception("Администратор с таким геотегом уже существует. Введите другой геотег:");
                                }

                                currentAdminGeoTag = geotag;

                                string adminPhoneMsg = "Введите номер телефона аккаунта админа:";
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Add(chat, "waitingModeratorToken", await sendTextButtonMessage(chat, adminPhoneMsg, "addAdminCancel"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }

                            State = BotState.waitingAdminPhoneNumber;
                            break;

                        case BotState.waitingAdminPhoneNumber:
                            try
                            {
                                string phone_number = msg.Trim();
                                ph_vl = new PhoneNumberValidator();
                                if (!ph_vl.IsValid(phone_number))
                                {
                                    await sendTextMessage(chat, ph_vl.Message);
                                    return;
                                }

                                var globals = GlobalSettings.getInstance();
                                UserAdmin admin = new UserAdmin(globals.push_api_id, globals.push_api_hash, phone_number, currentAdminGeoTag);
                                adminManager.Add(admin);
                                admin.Start();

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAdminVerificationCode:
                            try
                            {
                                var admin = adminManager.Get(currentAdminGeoTag);
                                if (admin != null)
                                    admin.SetVerifyCode(msg);                                
                            }
                            catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingNewSmartPushTimePeriod:
                            try
                            {
                                currentPushMessageTime = double.Parse(msg.Trim());
                                pushMessagesIds.Enqueue(update.Message.MessageId);
                                State = BotState.waitingNewSmartPushMessage;
                                await messagesProcessor.Back(chat);
                                string helloReqMsg = "Перешлите (forward) сюда push \U0001F9B5 сообщение:";
                                await messagesProcessor.Add(chat, "waitingNewSmartPushMessage", await sendTextButtonMessage(chat, helloReqMsg, "finishSmartPushAdding"));

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingNewSmartPushMessage:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);

                                SmartPushMessage pattern = await SmartPushMessage.Create(chat, bot, update.Message, chain.Name, currentPushMessageTime);

                                pushMessagesIds.Enqueue(update.Message.MessageId);

                                chain.AddSmartPushMessage(pattern.Clone(), moderationProcessor);
                                chainprocessor.Save();

                                int cntr = chain.PushData.Messages.Count;
                                await messagesProcessor.Add(chat, "smart_add", await sendTextButtonMessage(chat, $"Добавлено {currentPushMessageTime} сообщение. Введите период вывода следующего сообщения в часах:", "finishSmartPushAdding"));
                                State = BotState.waitingNewSmartPushTimePeriod;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAutoChangeOldText:
                            try
                            {
                                oldText = msg;
                                await messagesProcessor.Add(chat, "addAutoChangeNewText", await sendTextButtonMessage(chat, "Введите новую ссылку:", "back"));
                                State = BotState.waitingAutoChangeNewText;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAutoChangeNewText:
                            try
                            {
                                string newText = msg;
                                AutoChange autoChange = new AutoChange()
                                {
                                    OldText = oldText,
                                    NewText = newText
                                };

                                var chain = chainsProcessor.Get(currentChainID);
                                chain.AddAutoChange(currentBotName, autoChange);
                                chainprocessor.Save();
                                await messagesProcessor.Back(chat);
                                await showAutoChanges(chat, currentBotName);
                                await messagesProcessor.Add(chat, "addAutoChangeOldText", await sendTextButtonMessage(chat, "Введите ссылку, которую требуется заменить:", "back"));
                                State = BotState.waitingAutoChangeOldText;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAutoChangeDelete:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                int index = int.Parse(msg);
                                if (index > 0)
                                {                                    
                                    chain.RemoveAutoChange(currentBotName, index - 1);                                            
                                } else
                                {
                                    chain.ClearAutoChanges(currentBotName);
                                }
                                chainprocessor.Save();
                                await showDeleteAutoChanges(chat, currentBotName);

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                        case BotState.waitingAddAutoAnswer:
                            try
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                AutoAnswerMessage pattern = await AutoAnswerMessage.Create(chat, bot, update.Message, chain.Name);

                                chain.AddAutoAnswerMessage(pattern.Clone(), adminManager);
                                chainprocessor.Save();

                                int cntr = chain.PushData.Messages.Count;
                                //await messagesProcessor.Add(chat, "autoanswers_add", await sendTextMessage(chat, $"Автоответ добавлен."));
                                await messagesProcessor.Add(chat, "editAutoAnswersMenu", await sendTextButtonMessage(chat, "Автоответ добавлен. Что сделать?", "editAutoAnswersMenu"));
                                State = BotState.free;

                            } catch (Exception ex)
                            {
                                await sendTextMessage(chat, ex.Message);
                                return;
                            }
                            break;

                    }
                    break;
            }

            switch (State)
            {
                case BotState.waitingAddDaily:
                    try
                    {

                        var chain = chainsProcessor.Get(currentChainID);

                        DailyPushMessage pattern = await DailyPushMessage.Create(chat, bot, update.Message, chain.Name);

                        pushMessagesIds.Enqueue(update.Message.MessageId);

                        chain.AddDailyPushMessage(pattern.Clone(), moderationProcessor);
                        chainprocessor.Save();

                        int cntr = chain.DailyPushData.Messages.Count;
                        await messagesProcessor.Add(chat, "daily_add", await sendTextButtonMessage(chat, $"Добавлено {cntr} сообщений. Перешлите (forward) сюда следующее сообщение для цепочки:", "finishDailyPushAdding"));

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(chat, ex.Message);
                        return;
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
                        int index = 1;
                        foreach (var bot in chain.Bots)
                        {
                            botsinfo += $"{index}. {bot.ToString()}\n------\n";
                            index++;
                        }

                        string isActive = (chain.IsRunning) ? "АКТИВНА" : "НЕАКТИВНА";

                        string info = $"Id:{chain.Id}\n" +
                                      $"Имя:{chain.Name}\n" +
                                      $"Телефон шпиона:{chain.PhoneNumber}\n" +
                                      $"Боты:\n" + botsinfo +
                                      $"Период вывода сообщений:\n{chain.GetMessagingPeriod():0.0} мин.\n" +
                                      $"{isActive}\n";
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
                        chainsProcessor.Start(currentChainID);
                        chainsProcessor.Save();
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

                        //await messagesProcessor.Add(chat, "addOutputBot", await sendTextButtonMessage(chat, "Введите токен бота:", "addChainCancel"));
                        //State = BotState.waitingToken;

                        await messagesProcessor.Add(chat, "waitingOutputChannelId", await sendTextButtonMessage(chat, "Введите геотег бота:", "addChainCancel"));
                        State = BotState.waitingBotGeoTag;

                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "editBotGeotag":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        await showEditGeoTagBots(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Выберите");

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;
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
                        await messagesProcessor.Back(chat);
                        State = BotState.free;
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

                case "addReplacedWord":
                    try
                    {
                        await messagesProcessor.Add(chat, "addReplacedWord", await sendTextButtonMessage(chat, "Введите текст. Данный текст будет являться исключением, он будет удален из всех сообщений:", "back"));
                        State = BotState.waitingReplacedWord;
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteReplacedWord":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        await showReplacedWords(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Выберите исключение, который нужно удалить");

                    } catch (Exception ex)
                    {

                    }
                    break;

                case "saveReplacedWords":
                    State = BotState.free;
                    chainsProcessor.Save();
                    await messagesProcessor.Back(chat);
                    await messagesProcessor.Delete(chat, "addReplacedWord");
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;


                case "setMessagingPeriod":
                    try
                    {
                        await messagesProcessor.Add(chat, "setMessagingPeriod", await sendTextButtonMessage(chat, "Введите период вывода сообщений в минутах. Введите 0, если сообщения требуется выводить сразу:", "back"));
                        State = BotState.waitingMessagingPeriod;
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "newmoderator":
                    State = BotState.waitingModeratorGeoTag;
                    try
                    {
                        await messagesProcessor.Add(chat, "newmoderator", await sendTextButtonMessage(chat, "Введите геотег нового модератора", "addModeratorCancel"));
                        await messagesProcessor.Delete(chat, "/mymoderators");
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteModerator":
                    try
                    {
                        moderationProcessor.Delete(currentModeratorGeoTag);
                        await messagesProcessor.Back(chat);
                        await messagesProcessor.Back(chat);
                        await showMyModerators(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Модератор удален");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addModeratorCancel":
                    try
                    {

                        switch (State)
                        {
                            case BotState.waitingModeratorGeoTag:
                                await messagesProcessor.Back(chat);
                                currentModeratorGeoTag = "";
                                return;

                            case BotState.waitingModeratorToken:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                break;

                            case BotState.waitingModeratorHelloMessage:
                            case BotState.waitingModerarotAlternativeLink:
                            case BotState.waitingModeratorByeMessage:
                                //await messagesProcessor.Back(chat);
                                currentModeratorGeoTag = "";
                                break;
                        }

                        State = BotState.free;
                        if (!string.IsNullOrEmpty(currentModeratorGeoTag))
                        {
                            try
                            {
                                var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                                moderationProcessor.Delete(currentModeratorGeoTag);
                            } catch { };
                        }

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }

                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "newadmin":
                    State = BotState.waitingAdminGeoTag;
                    try
                    {
                        await messagesProcessor.Add(chat, "newmoderator", await sendTextButtonMessage(chat, "Введите геотег нового администратора", "addAdminCancel"));
                        await messagesProcessor.Delete(chat, "/myadministrators");
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addAdminCancel":
                    try
                    {

                        switch (State)
                        {
                            case BotState.waitingAdminGeoTag:
                                await messagesProcessor.Back(chat);
                                return;

                            case BotState.waitingAdminPhoneNumber:
                                await messagesProcessor.Back(chat);
                                await messagesProcessor.Back(chat);
                                return;
                        }

                        State = BotState.free;
                        if (!string.IsNullOrEmpty(currentAdminGeoTag))
                        {
                            try
                            {
                                adminManager.Delete(currentAdminGeoTag);
                            } catch { };
                        }
                        currentAdminGeoTag = "";

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteAdmin":
                    try
                    {
                        adminManager.Delete(currentAdminGeoTag);
                        await messagesProcessor.Back(chat);
                        await messagesProcessor.Back(chat);
                        await showMyAdmins(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id, "Админ удален");
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "restartAdmin":
                    try
                    {
                        var admin = adminManager.Get(currentAdminGeoTag);

                        if (admin != null)
                        {

                            admin.Stop();
                            await admin.Start();

                            await messagesProcessor.Back(chat);
                            await messagesProcessor.Back(chat);
                            await showMyAdmins(chat);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Админ перезапущен");
                        }

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "stopAdmin":
                    try
                    {
                        var admin = adminManager.Get(currentAdminGeoTag);
                        if (admin != null)
                        {
                            admin.Stop();
                            await messagesProcessor.Back(chat);
                            await messagesProcessor.Back(chat);
                            await showMyAdmins(chat);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Админ остановлен");
                        }

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "finishEddingGreetings":
                    State = BotState.waitingModeratorByeMessage;
                    await messagesProcessor.Back(chat);
                    await messagesProcessor.Delete(chat, "finishEddingGreetings");
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;


                case "finishEddingJoinReply":
                    State = BotState.free;
                    await messagesProcessor.Back(chat);
                    await messagesProcessor.Delete(chat, "finishEddingJoinReply");
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "editJoinMessage":
                    //await messagesProcessor.Back(chat);
                    //await messagesProcessor.Delete(chat, "editJoinMessage");                    
                    await messagesProcessor.Add(chat, "editJoinMessageMenu", await sendTextButtonMessage(chat, "Для того чтобы заменить Join сообщение нажмите Добавить", "editJoinMessageMenu"));
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "editPreJoinMessage":
                    //await messagesProcessor.Back(chat);
                    //await messagesProcessor.Delete(chat, "editJoinMessage");                    
                    await messagesProcessor.Add(chat, "editPreJoinMessageMenu", await sendTextButtonMessage(chat, "Для того чтобы заменить Кружок нажмите Добавить", "editPreJoinMessageMenu"));
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "editPushStart":
                    try
                    {
                        var moderator = moderationProcessor.Get(currentModeratorGeoTag);

                        if (moderator != null)
                        {
                            //var usePushStrart = moderator.UsePushStartButton;

                            var usePushStart = moderationProcessor.Greetings(currentModeratorGeoTag).UsePushStartButton;

                            await showMyModeratorsPushStart(chat, usePushStart);
                            await bot.AnswerCallbackQueryAsync(query.Id);
                        }


                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }

                    break;

                case "editLeaveMessage":
                    //await messagesProcessor.Back(chat);
                    //await messagesProcessor.Delete(chat, "editLeaveMessage");
                    await messagesProcessor.Add(chat, "editLeaveMessageMenu", await sendTextButtonMessage(chat, "Для того чтобы заменить Leave сообщение нажмите Добавить", "editLeaveMessageMenu"));
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "editModeratorGeoTag":
                    await messagesProcessor.Add(chat, "editModeratorGeoTag", await sendTextButtonMessage(chat, $"Текущий геотег модератора - {currentModeratorGeoTag}. Введите новый геотег:", "back"));
                    State = BotState.waitingModeratorGeoTagEdit;
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "checkLeadAlgorithm":
                    try
                    {
                        var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                        await showMyModeratorsLeadType(chat, moderator.LeadType);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "backToModeratorShow":
                    try
                    {
                        await messagesProcessor.Back(chat);
                        var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                        string m = $"Выбран модератор {moderator.GeoTag}. Что сделать?";
                        await messagesProcessor.Add(chat, "editModerator", await sendTextButtonMessage(chat, m, "editModerator"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "backToMyModerators":
                    try
                    {
                        await messagesProcessor.Back(chat);
                        await showMyModerators(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "addAutoChange":
                    try
                    {

                        var chain = chainsProcessor.Get(currentChainID);
                        await showAddAutochangeBots(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "deleteAutoChange":
                    try
                    {

                        //await showDeleteAutoChanges(chat);

                        //State = BotState.waitingAutoChangeDelete;

                        //await bot.AnswerCallbackQueryAsync(query.Id);

                        var chain = chainsProcessor.Get(currentChainID);
                        await showDeleteAutochangeBots(chat, chain);
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "editDailyPushes":
                    try
                    {
                        await messagesProcessor.Add(chat, "editDailyPushesMenu", await sendTextButtonMessage(chat, "Настройка ежедневных уведомлений:", "editDailyPushesMenu"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "daily_add":
                    try
                    {
                        await messagesProcessor.Add(chat, "daily_add", await sendTextButtonMessage(chat, "Перешлите (forward) сюда исходное сообщение для цепочки:", "finishDailyPushAdding"));
                        State = BotState.waitingAddDaily;
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "daily_delete":
                    try
                    {
                        //var chain = chainsProcessor.Get(currentChainID);
                        //chain.ClearDailyPushMessages(moderationProcessor);
                        //chainsProcessor.Save();
                        //await bot.AnswerCallbackQueryAsync(query.Id);
                        //await sendTextMessage(chat, "Ежедневные сообщения удалены");

                        await deleteMyModeratorsDailyPushMessages(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "daily_show":
                    try
                    {
                        await showMyModeratorsDailyPushesMessages(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "finishDailyPushAdding":
                    try
                    {
                        State = BotState.free;

                        await messagesProcessor.Back(chat);

                        while (pushMessagesIds.Count > 0)
                        {
                            var id = pushMessagesIds.Dequeue();
                            try
                            {
                                await bot.DeleteMessageAsync(chat, id);

                            } catch (Exception ex)
                            {

                            }
                        }

                        await messagesProcessor.Add(chat, "editDailyPushesMenu", await sendTextButtonMessage(chat, "Настройка ежедневных уведомлений:", "editDailyPushesMenu"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "finishDailyPushShow":
                    try
                    {
                        while (pushMessagesIds.Count > 0)
                        {
                            await bot.DeleteMessageAsync(chat, pushMessagesIds.Dequeue());
                        }
                        await bot.AnswerCallbackQueryAsync(query.Id);
                        await messagesProcessor.Back(chat);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }

                    break;


                case "editSmartPushes":
                    try
                    {
                        await messagesProcessor.Add(chat, "editSmartPushesMenu", await sendTextButtonMessage(chat, "Настройка smart уведомлений:", "editSmartPushesMenu"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;


                case "smart_add":
                    try
                    {
                        await messagesProcessor.Add(chat, "smart_add", await sendTextButtonMessage(chat, "Введите период вывода сообщения в часах:", "finishSmartPushAdding"));
                        State = BotState.waitingNewSmartPushTimePeriod;
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "smart_delete":
                    try
                    {
                        await deleteMyModeratorsSmartPushMessages(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "finishSmartPushAdding":
                    try
                    {
                        State = BotState.free;

                        await messagesProcessor.Back(chat);

                        while (pushMessagesIds.Count > 0)
                        {
                            var id = pushMessagesIds.Dequeue();
                            try
                            {
                                await bot.DeleteMessageAsync(chat, id);

                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        await messagesProcessor.Add(chat, "editSmartPushesMenu", await sendTextButtonMessage(chat, "Настройка smart-уведомлений:", "editSmartPushesMenu"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "smart_show":
                    try
                    {
                        await showMyModeratorsSmartPushesMessages(chat);
                        await bot.AnswerCallbackQueryAsync(query.Id);
                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "pushstart_on":
                    try
                    {
                        var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                        if (moderator != null)
                        {
                            bool ps = true;
                            moderator.Greetings.UsePushStartButton = ps;
                            await showMyModeratorsPushStart(chat, ps);
                            await bot.AnswerCallbackQueryAsync(query.Id);
                            moderationProcessor.Save();
                        }

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "pushstart_off":
                    try
                    {
                        var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                        if (moderator != null)
                        {
                            bool ps = false;
                            moderator.Greetings.UsePushStartButton = ps;
                            await showMyModeratorsPushStart(chat, ps);
                            await bot.AnswerCallbackQueryAsync(query.Id);
                            moderationProcessor.Save();

                        }
                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "editAutoAnswers":
                    try
                    {

                        await messagesProcessor.Add(chat, "editAutoAnswersMenu", await sendTextButtonMessage(chat, "Настройка автоответов:", "editAutoAnswersMenu"));
                        await bot.AnswerCallbackQueryAsync(query.Id);

                    } catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }
                    break;

                case "autoanswers_show":

                    try
                    {

                        var chain = chainsProcessor.Get(currentChainID);
                        var messages = chain.AutoAnswerData.Messages;

                        await bot.AnswerCallbackQueryAsync(query.Id);                        
                        int cntr = 0;

                        if (messages.Count > 0)
                        {
                            foreach (var message in messages)
                            {
                                message.fileId = null;
                                pushMessagesIds.Enqueue(await message.Send(chat, bot));
                                cntr++;
                            }
                            await messagesProcessor.Add(chat, "finishAutoAnswersShow", await sendTextButtonMessage(chat, $"Показано {cntr} сообщений", "back"));
                        }
                        else
                        {
                            await sendTextMessage(query.Message.Chat.Id, $"Для цепочки {chain.Name} не установлены автоответы");
                        }


                    }
                    catch (Exception ex)
                    {
                        await sendTextMessage(query.Message.Chat.Id, ex.Message);
                    }


                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "autoanswers_add":
                    await messagesProcessor.Add(chat, "autoanswers_add", await sendTextButtonMessage(chat, "Перешлите сюда сообщение-автоответ:", "back"));
                    State = BotState.waitingAddAutoAnswer;
                    await bot.AnswerCallbackQueryAsync(query.Id);
                    break;

                case "autoanswers_delete":
                    try
                    {
                        var chain = chainsProcessor.Get(currentChainID);
                        chain.ClearAutoAnswerMessage(adminManager);
                        await sendTextMessage(query.Message.Chat.Id, $"Автоответ удален для всей цепочки");
                    } catch (Exception ex)
                    {
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

                    if (data.Contains("bot_delete_"))
                    {
                        try
                        {
                            string name = data.Replace("bot_delete_", "");
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

                    if (data.Contains("bot_geotag_"))
                    {
                        try
                        {
                            string name = data.Replace("bot_geotag_", "");
                            currentBotName = name;
                            await messagesProcessor.Add(chat, "bot_geotag_", await sendTextButtonMessage(chat, $"Введите новый геотег бота {name}:", "back"));
                            State = BotState.waitingBotGeoTagEdit;
                            await bot.AnswerCallbackQueryAsync(query.Id);
                            
                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("bot_+autochange_"))
                    {
                        try
                        {
                            string name = data.Replace("bot_+autochange_", "");
                            currentBotName = name;
                            Console.WriteLine(name);
                            //await messagesProcessor.Back(chat);
                            await showAutoChanges(chat, currentBotName);
                            await messagesProcessor.Add(chat, "addAutoChangeOldText", await sendTextButtonMessage(chat, "Введите ссылку, которую требуется заменить:", "back"));
                            State = BotState.waitingAutoChangeOldText;
                            await bot.AnswerCallbackQueryAsync(query.Id);

                        }
                        catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("bot_-autochange_"))
                    {
                        try
                        {
                            var chain = chainsProcessor.Get(currentChainID);
                            string name = data.Replace("bot_-autochange_", "");
                            currentBotName = name;
                            State = BotState.waitingAutoChangeDelete;
                            //await messagesProcessor.Back(chat);
                            await showDeleteAutoChanges(chat, currentBotName);
                            await bot.AnswerCallbackQueryAsync(query.Id);

                        } catch (Exception ex)
                        {

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


                    if (data.Contains("replacedWords_"))
                    {
                        try
                        {
                            string sindex = data.Replace("replacedWords_", "");
                            int index = int.Parse(sindex);
                            var chain = chainsProcessor.Get(currentChainID);
                            chain.RemoveReplacedWord(index);
                            chainprocessor.Save();
                            await showReplacedWords(chat, chain);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Исключение удалено");

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

                    if (data.Contains("moderator_"))
                    {
                        try
                        {

                            currentModeratorGeoTag = data.Replace("moderator_", "");
                            var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                            string m = $"Выбран модератор {moderator.GeoTag}. Что сделать?";
                            await messagesProcessor.Add(chat, "editModerator", await sendTextButtonMessage(chat, m, "editModerator"));
                            await bot.AnswerCallbackQueryAsync(query.Id);

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("clearReplacedWords"))
                    {
                        try
                        {
                            var chain = chainsProcessor.Get(currentChainID);
                            chain.ClearReplacedWords();
                            chainprocessor.Save();
                            await messagesProcessor.Back(chat);
                            await bot.AnswerCallbackQueryAsync(query.Id, "Исключения удалены");

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("greetings"))
                    {
                        try
                        {
                            string[] splt = data.Split("_");
                            string type = splt[1];
                            string cmd = splt[2];

                            switch (cmd)
                            {
                                case "show":
                                    try
                                    {
                                        var greetings = moderationProcessor.Greetings(currentModeratorGeoTag);
                                        TextMessage greetingsMsg = null;
                                        switch (type)
                                        {
                                            case "join":
                                                var msg = greetings.HelloMessage;
                                                await msg.Send(Id, bot);
                                                //greetingsMsg = greetings.HelloMessage;

                                                break;
                                            case "leave":
                                                greetingsMsg = greetings.ByeMessage;
                                                await bot.SendTextMessageAsync(
                                                   Id,
                                                   text: greetingsMsg.Text,
                                                   replyMarkup: greetingsMsg.ReplyMarkup,
                                                   entities: greetingsMsg.Entities,
                                                   disableWebPagePreview: false,
                                                   cancellationToken: cancellationToken);
                                                break;
                                            case "prejoin":                                                
                                                await moderationProcessor.Greetings(currentModeratorGeoTag).PreJoinMessage.Send(chat, bot);                                                
                                                break;
                                            default:
                                                break;
                                        }

                                        //await bot.SendTextMessageAsync(
                                        //       Id,
                                        //       text: greetingsMsg.Text,
                                        //       replyMarkup: greetingsMsg.ReplyMarkup,
                                        //       entities: greetingsMsg.Entities,
                                        //       disableWebPagePreview: false,
                                        //       cancellationToken: cancellationToken);

                                    } catch (Exception ex)
                                    {
                                        await sendTextMessage(Id, "Сообщение не задано");
                                    }
                                    await bot.AnswerCallbackQueryAsync(query.Id);
                                    break;

                                case "add":
                                    switch (type)
                                    {
                                        case "join":
                                            State = BotState.waitingModeratorHelloMessageEdit;
                                            string helloReqMsg = "Перешлите (forward) сюда приветственное \U0001F4B0 сообщение, для отмены нажмите Завершить";
                                            await messagesProcessor.Add(chat, "waitingModeratorHelloMessage", await sendTextButtonMessage(chat, helloReqMsg, "finishEddingGreetings"));
                                            break;
                                        case "leave":
                                            State = BotState.waitingModeratorByeMessageEdit;
                                            string byeReqMsg = "Перешлите (forward) сюда прощальное \U000026B0 сообщение, для отмены нажмите Завершить";
                                            await messagesProcessor.Add(chat, "waitingModeratorByeMessage", await sendTextButtonMessage(chat, byeReqMsg, "finishEddingGreetings"));
                                            break;
                                        case "prejoin":
                                            State = BotState.waitingModeratorPreJoinMessageEdit;
                                            string preJoinReqMsg = "Перешлите (forward) сюда кружок, для отмены нажмите Завершить";
                                            await messagesProcessor.Add(chat, "waitingModeratorPreJoinMessage", await sendTextButtonMessage(chat, preJoinReqMsg, "finishEddingGreetings"));
                                            break;
                                        default:
                                            break;
                                    }
                                    await bot.AnswerCallbackQueryAsync(query.Id);
                                    break;

                                case "delete":
                                    switch (type)
                                    {
                                        case "join":
                                            //moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessage = new();
                                            moderationProcessor.Greetings(currentModeratorGeoTag).DeleteHelloMessage();
                                            break;
                                        case "leave":
                                            moderationProcessor.Greetings(currentModeratorGeoTag).ByeMessage = new();
                                            break;
                                        case "prejoin":
                                            moderationProcessor.Greetings(currentModeratorGeoTag).DeletePreJoinMessage();
                                            break;
                                        default:
                                            break;
                                    }
                                    moderationProcessor.Save();
                                    await sendTextMessage(Id, "Сообщение удалено");
                                    await bot.AnswerCallbackQueryAsync(query.Id);
                                    break;

                                default:
                                    break;
                            }

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }


                    if (data.Contains("pushstart"))
                    {
                        try {
                            string[] splt = data.Split("_");

                            switch (splt[1])
                            {

                                case "add":
                                    State = BotState.waitingModeratorJoinMessageReplyEdit;
                                    string helloReqMsg = "Перешлите (forward) сюда ответ на нажатие push-кнопки, для отмены нажмите Завершить";
                                    await messagesProcessor.Add(chat, "waitingModeratorJoinMessageReply", await sendTextButtonMessage(chat, helloReqMsg, "finishEddingJoinReply"));
                                    break;

                                case "remove":
                                    moderationProcessor.Greetings(currentModeratorGeoTag).HelloMessageReply = new();
                                    await sendTextMessage(Id, "Сообщение удалено");
                                    break;

                                case "show":
                                    try
                                    {
                                        GreetingsData greetings = moderationProcessor.Greetings(currentModeratorGeoTag);
                                        await bot.SendTextMessageAsync(
                                               Id,
                                               text: greetings.HelloMessageReply.Text,
                                               replyMarkup: greetings.HelloMessageReply.ReplyMarkup,
                                               entities: greetings.HelloMessageReply.Entities,
                                               disableWebPagePreview: false,
                                               cancellationToken: cancellationToken);
                                    } catch (Exception ex)
                                    {
                                        await sendTextMessage(Id, "Сообщение не задано");
                                    }
                                    break;

                            }

                            await bot.AnswerCallbackQueryAsync(query.Id);
                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("admin_"))
                    {
                        try
                        {

                            if (!data.Contains("autoanswer"))
                            {

                                currentAdminGeoTag = data.Replace("admin_", "");
                                var admin = adminManager.Get(currentAdminGeoTag);
                                await showAdminEdit(chat, admin);
                            }
                            else
                            {

                                var splt = data.Split("_");
                                currentAdminGeoTag = splt[1];
                                var admin = adminManager.Get(currentAdminGeoTag);
                                admin.NeedAutoAnswer = (splt[3].Equals("on")) ? true : false;
                                await showAdminEdit(chat, admin);

                            }

                            //string m = $"Выбран администратор {admin.geotag}. Что сделать?";
                            //await messagesProcessor.Add(chat, "editAdmin", await sendTextButtonMessage(chat, m, "editAdmin"));

                            await bot.AnswerCallbackQueryAsync(query.Id);

                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("moderators_show_daily_pushes_"))
                    {
                        try
                        {
                            string geotag = data.Replace("moderators_show_daily_pushes_", "");
                            var pushes = moderationProcessor.DailyPushData(geotag).Messages;
                            await bot.AnswerCallbackQueryAsync(query.Id);

                            pushMessagesIds.Clear();
                            int cntr = 0;
                            if (pushes.Count > 0)
                            {
                                foreach (var push in pushes)
                                {
                                    push.fileId = null;                                    
                                    pushMessagesIds.Enqueue(await push.Send(chat, bot));
                                    cntr++;
                                }

                                await messagesProcessor.Add(chat, "finishDailyPushShow", await sendTextButtonMessage(chat, $"Показано {cntr} сообщений", "finishDailyPushShow"));

                            } else
                            {
                                await sendTextMessage(query.Message.Chat.Id,   $"Для модератора {geotag} не установлены ежедневные push-сообщения");
                            }


                        } catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("moderators_delete_daily_pushes_"))
                    {
                        try
                        {
                            string geotag = data.Replace("moderators_delete_daily_pushes_", "");

                            if (geotag.Equals("all"))
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.ClearDailyPushMessages(moderationProcessor);
                                chainsProcessor.Save();
                                await bot.AnswerCallbackQueryAsync(query.Id);
                                await sendTextMessage(chat, "Ежедневные push-сообщения удалены для всех модерторов");
                                return;
                            }

                            moderationProcessor.DailyPushData(geotag).Messages.Clear();
                            moderationProcessor.Save();
                            await sendTextMessage(chat, $"Ежедневные push-сообщения удалены для модератора {geotag}");

                        }
                        catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("moderators_delete_smart_pushes_"))
                    {
                        try
                        {
                            string geotag = data.Replace("moderators_delete_smart_pushes_", "");

                            if (geotag.Equals("all"))
                            {
                                var chain = chainsProcessor.Get(currentChainID);
                                chain.ClearSmartPushMessages(moderationProcessor);
                                chainsProcessor.Save();
                                await bot.AnswerCallbackQueryAsync(query.Id);
                                await sendTextMessage(chat, "Smart-сообщения удалены для всех модерторов");
                                return;
                            }

                            moderationProcessor.SmartPushData(geotag).Messages.Clear();
                            moderationProcessor.Save();
                            await sendTextMessage(chat, $"Smart-сообщения удалены для модератора {geotag}");

                        }
                        catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("moderators_show_smart_pushes_"))
                    {
                        try
                        {
                            string geotag = data.Replace("moderators_show_smart_pushes_", "");
                            var pushes = moderationProcessor.SmartPushData(geotag).Messages;
                            await bot.AnswerCallbackQueryAsync(query.Id);

                            //pushMessagesIds.Clear();
                            int cntr = 0;
                            if (pushes.Count > 0)
                            {
                                foreach (var push in pushes)
                                {
                                    push.fileId = null;
                                    //await sendTextMessage(chat, $"{push}");

                                    var msg = await sendTextMessage(chat, $"{push.TimePeriod} часовое сообщение:");
                                    pushMessagesIds.Enqueue(msg.MessageId);
                                    pushMessagesIds.Enqueue(await push.Send(chat, bot));
                                    cntr++;
                                }

                                await messagesProcessor.Add(chat, "finishSmartPushShow", await sendTextButtonMessage(chat, $"Показано {cntr} сообщений", "finishSmartPushShow"));

                            }
                            else
                            {
                                await sendTextMessage(query.Message.Chat.Id, $"Для модератора {geotag} не установлены ежедневные smart-сообщения");
                            }


                        }
                        catch (Exception ex)
                        {
                            await sendTextMessage(query.Message.Chat.Id, ex.Message);
                        }
                    }

                    if (data.Contains("lead_algo_"))
                    {
                        try
                        {
                            string alg = data.Replace("lead_algo_", "");
                            int val = int.Parse(alg);
                            LeadAlgorithmType algorithm = (LeadAlgorithmType)val;
                            await bot.AnswerCallbackQueryAsync(query.Id);
                            var moderator = moderationProcessor.Get(currentModeratorGeoTag);
                            moderator.LeadType = algorithm;
                            await showMyModeratorsLeadType(chat, moderator.LeadType);                            

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
