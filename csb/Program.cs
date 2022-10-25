using csb.bot_manager;
using csb.bot_poster;
using csb.usr_listener;
using csb.bot_moderator;
using System;
using System.Collections;
using System.Collections.Generic;
using csb.matching;
using csb.usr_push;

namespace csb
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Вдудь 3.1.0");
            BotManager manager = new BotManager();
            manager.Start();

            //var manager = new TGUserManager<TestUser>("push.json");            

            //foreach (var user in manager.Users)
            //{
            //    user.VerifyCodeRequestEvent += User_VerifyCodeRequestEvent;
            //}

            //manager.StartAll();

            string text = "";
            do
            {
                text = Console.ReadLine();
            } while (!text.Equals("quit"));
        }

        private static void User_VerifyCodeRequestEvent(ITGUser arg)
        {
            Console.WriteLine($"Введите код для {arg.phone_number}");
        }

        
    }
}
