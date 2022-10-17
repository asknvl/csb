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


            Console.WriteLine("Вдудь 3.0.0");

            BotManager manager = new BotManager();
            manager.Start();

            //csb.server.TGStatApi api = new csb.server.TGStatApi("http://136.243.74.153:4000");
            //var res = api.GetFollowerGeoTags(1481806946);


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
