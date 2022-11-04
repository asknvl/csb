﻿using csb.addme_service;
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
        waitingVictimChannelLink,
        waitingOutputChannelLink,
        waitingVerificationCode,

        waitingAddInputChannel,

        waitingFilteredWord,

        waitingMessagingPeriod,

        waitingModeratorGeoTag,
        waitingModeratorToken,

        waitingModeratorHelloMessage,                        
        waitingModerarotAlternativeLink,
        waitingModeratorByeMessage,

        waitingModeratorHelloMessageEdit,
        waitingModeratorByeMessageEdit,

        waitingAdminGeoTag,
        waitingAdminPhoneNumber,
        waitingAdminVerificationCode,

        waitingNewPushTimePeriod,
        waitingNewPushMessage,

        waitingReplacedWord,


    }
    
    

    public class BotManager
    {

        #region vars
        ITelegramBotClient bot;
        CancellationToken cancellationToken;
        //ChainProcessor chainsProcessor = new ChainProcessor("chains.json");        
        AddMeService addMe = AddMeService.getInstance();
        UserManager userManager;

        #endregion

        public BotManager()
        {


#if DEBUG
            bot = new TelegramBotClient("5670690683:AAHxCfqo0N-S64tyrNkTKwlz-TfHb11R70Q");
#elif LATAM
            //Latam
            bot = new TelegramBotClient("5597155386:AAEvPn9KUuWRPCECuOTJDHdh6RiY_IVbpWM");             
#elif VESTNIK
            //Вестник
            bot = new TelegramBotClient("5417889302:AAG2sMp32gXlzfl6HnEvB2VmVXfAR_7G274");            
#endif


            //chainsProcessor.Load();

            userManager = new UserManager(bot, cancellationToken);
            userManager.Init();

            //chainsProcessor.StartAll(); 
            
        }

       

        public async void Start()
        {
            cancellationToken = new CancellationToken();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery }
            };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, new CancellationToken());            
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {

            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    //await processCallbackQuery(update.CallbackQuery);
                    await userManager.UpdateCallbackQuery(update.CallbackQuery);
                    break;

                    case UpdateType.Message:

                    await Task.Run(async () => {

                        var chat = update.Message.Chat.Id;
                        string msg = update.Message.Text;
                        if (msg == null)
                            return;

                        if (msg.Equals("/addme"))
                        {
                            try
                            {
                                addMe.Add(chat);
                            } catch (Exception ex)
                            {
                                await bot.SendTextMessageAsync(
                                  chatId: chat,
                                  text: ex.Message,
                                  cancellationToken: cancellationToken);
                            }
                            return;
                        }

                        if (msg.Equals("7777"))
                        {
                            string name = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";
                            userManager.Add(chat, name);

                            string helloText = "Вас приветствует бот Вдуть 2.0!\n" +
                                "Бот позволяет копировать чужие каналы (входные каналы) и заменять в них ссылки.\n" +
                                "Для копирования канала нужно создать цепочку.\n" +
                                "Цепочка состоит из пользователя-шпиона и выводных ботов:\n" +
                                "-Шпион должены быть подписан на чужой канал;\n" +
                                "-Выводной бот должен быть администратором одного канала, в который будут копироваться данные (выходной канал);\n" +
                                "-Выходных каналов может быть несколько.\n\n" +
                                "/mychains - управление цепочками\n" +
                                "/mymoderators - управление ботами-модераторами\n";

                            await bot.SendTextMessageAsync(
                                   chatId: chat,
                                   text: helloText,                                   
                                   cancellationToken: cancellationToken);
                            return;
                            
                        }

                        if (!userManager.Check(chat) && !msg.Equals("/start"))
                        {
                            await bot.SendTextMessageAsync(
                                   chatId: chat,
                                   text: "Нет доступа",
                                   cancellationToken: cancellationToken);
                            return;
                        }

                        if (msg.Equals("/start"))
                        {
                            await bot.SendTextMessageAsync(
                                  chatId: chat,
                                  text: "Введите пароль",
                                  cancellationToken: cancellationToken);
                            return;
                        }

                        await userManager.UpdateMessage(update);

                    });


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
