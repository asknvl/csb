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
using csb.settings;
using capi_test.capi;

namespace csb
{

    internal class Program
    {

        static void Main(string[] args)
        {

            WTelegram.Helpers.Log = (x, y) => {
            };

            GlobalSettings settings = GlobalSettings.getInstance();

            Console.WriteLine("Вдудь 3.5.5");
            Console.WriteLine($"Офис:{settings.office}");
            BotManager manager = new BotManager();
            manager.Start();

            //string token = "EAACeORAy5ggBAAQK6MxX6quoDg8Bv16AZCtQGO62PWBjBOiJjb5dBlago7ZCvAQXA1loXEhtb6UjkdxYFWiYUm7HL0DVc4fSi6oeYVbQvAgBmGCtfskhy9UjUqfZC3bN6bekSox3oABOGnND2Nx22AMeH1MIjgiXd17CqqqZBZAWeMzwpvwZC7jdXYxuQnuQYZD";
            //string pixel = "1052154679095265";

            //string ua = $"Mozilla/5.0 (Windows NT 10.0; Win64, x64);\r\nAppleWebKit/537.36 (KHTML, like Gecko);\r\nChrome/87.0.4280.141;\r\nSafari/537.36.";
            //string ip = "46.175.145.172";

            //string fbc = "IwAR2A5JjBweXSpeeNOkD1s4kplOIZtJqUOhL4QpALl_tAuB0fgb7CPZoH8fc";

            //CAPI cAPI= new CAPI();



            //Task.Run(async () =>
            //{
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Alex Willis", "Bert", ua, ip, fbc, "TEST84112");
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Pere", "Meiz", ua, ip, fbc, "TEST84112");
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Claus Shulze", null, ua, fbc);
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Василий", "Петров", ua, ip, fbc);
            //    await cAPI.MakeLeadEvent(pixel, token, 178999888, "Семен", "Шариков");
            //});



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
