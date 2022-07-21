using csb.bot_manager;
using csb.bot_poster;
using csb.usr_listener;
using csb.bot_moderator;
using System;

namespace csb
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Вдудь 2.0.4");

            BotManager manager = new BotManager();
            manager.Start();

            //BotModerator moderator = new BotModerator("5452060432:AAGL0eApCkj5lFT6vivJYMU87u6ctDERBk4", "PER1X");
            //moderator.Start();


            Console.ReadLine();
        }
    }
}
