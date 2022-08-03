using csb.bot_manager;
using csb.bot_poster;
using csb.usr_listener;
using csb.bot_moderator;
using System;
using System.Collections;
using System.Collections.Generic;
using csb.matching;

namespace csb
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Вдудь 2.0.8");

            BotManager manager = new BotManager();
            manager.Start();


            //ITextMatchingAnalyzer analyzer = new TextMatchingAnalyzer(4);

            //analyzer.Add("aaa, bbb, ccc");
            //analyzer.Add("ddd eee fff.");
            //analyzer.Add("ggg, hhh.iii,\n");
            //analyzer.Add("ggg\n hhh. iii");

            //analyzer.Check("aaa bbb ccc");





            Console.ReadLine();
        }
    }
}
