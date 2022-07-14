using csb.bot_manager;
using csb.bot_poster;
using csb.usr_listener;
using System;

namespace csb
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //UserListener user = new UserListener("+79256186936");
            //BotPoster_api bot = new BotPoster_api("5565803263:AAEea9w5P3EgIc94Xng4vpItImVl2Jw1TYY");
            //bot.OutputChannelID = -1001783366577;
            //bot.Start();
            //user.Start().Wait();

            Console.WriteLine("Вдудь 2.0.0");

            BotManager manager = new BotManager();
            manager.Start();

            Console.ReadLine();
        }
    }
}
