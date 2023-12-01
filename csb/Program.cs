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
using csb.capi;

namespace csb
{

    internal class Program
    {

        static void Main(string[] args)
        {

#if RELEASE
            WTelegram.Helpers.Log = (x, y) =>
            {
            };
#endif

            GlobalSettings settings = GlobalSettings.getInstance();

            Console.WriteLine("Вдудь 3.5.9");
            Console.WriteLine($"Офис:{settings.office}");
            BotManager manager = new BotManager();
            manager.Start();

            //string token = "EAAeZBUdwokCsBO12IetmPs9eZADAjwn9q5NPD6Qrd6jts1x2cdBsWLqGAq9sHeSdgscU0SZCbMpGTyseC1Skv0lyEMgNhXRGP6CzLza6Y6QtKWe86Aj6IIZC15zMuTn7YFt1NGIc3RkeDFgCoK97GWHuSFnj0bXwdt2zyT8jDapDi1mMolVK38UXiM84TmjGfAZDZD";
            //string pixel = "467020078954092";

            //string ua = $"Mozilla/5.0 (Windows NT 10.0; Win64, x64);\r\nAppleWebKit/537.36 (KHTML, like Gecko);\r\nChrome/87.0.4280.141;\r\nSafari/537.36.";
            //string ip = "46.175.145.172";

            //string fbc = "IwAR2A5JjBweXSpeeNOkD1s4kplOIZtJqUOhL4QpALl_tAuB0fgb7CPZoH8fc";

            //ICAPI cAPI = new CAPIv2();

            //string fbc = "PAAaYqQTUcjjQI3vp0FuVt2SvHCc-G7263_3lrg0I2Mkz7m4zgIBGuVmULIeU_aem_AZ2Vls1FzWN-zulxkWC13q3e0U_SvENvUGMRCDdszEitq1yX0KkUck-cgtBX5MqvoMYwngfW7eMPFbZhgcrWj1jg";
            //string fbp = "fb.1.1696523641071.1250272867";

            //Task.Run(async () =>
            //{
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Alex Willis", "Bert", ua, ip, fbc, "TEST84112");
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Pere", "Meiz", ua, ip, fbc, "TEST84112");
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Claus Shulze", null, ua, fbc);
            //    //await cAPI.MakeLeadEvent(pixel, token, 178999888, "Василий", "Петров", ua, ip, fbc);
            //    await cAPI.MakeContactEvent(pixel, token, 178999888, "Семен", "Шариков", ua, ip, fbc, fbp, "TEST98476");
            //});

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
