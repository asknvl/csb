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
using System.Threading.Tasks;

namespace csb
{

    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Вдудь 3.1.9_2");
            BotManager manager = new BotManager();
            manager.Start();

            //Task.Run(async () => {
            //    ITGFollowersStatApi statApi = new TGFollowersStatApi_v2("http://136.243.74.153:4000");
            //    //var res = await statApi.GetFollowerGeoTags(1481806946);

            //    //string date_to = DateTime.Now.ToString("yyyy-MM-dd");
            //    //var res = await statApi.GetNoFeedbackFollowers("PERX01", date_to, date_to);

            //    //await statApi.MarkFollowerWasPushed("PERX01", 1481806946, 9, true);

            //    //var res = await statApi.IsSubscriptionAvailable("PERX01", 1481806946);

            //    //var res = await statApi.GetUsersNeedDailyPush("PERX01", 9);
            //});           

            string text = "";
            do
            {
                text = Console.ReadLine();
            } while (!text.Equals("quit"));

        }
    }
}
