using csb.bot_manager;
using csb.bot_poster;
using csb.usr_listener;
using csb.bot_moderator;
using System;
using System.Collections;
using System.Collections.Generic;
using csb.matching;
using csb.usr_push;
using csb.server;
using System.Linq;

namespace csb
{

    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Вдудь 3.1.9_1");
            BotManager manager = new BotManager();
            manager.Start();

            string text = "";
            do
            {
                text = Console.ReadLine();
            } while (!text.Equals("quit"));

        }
    }
}
