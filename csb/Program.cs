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
using System.Threading.Tasks;
using System.Threading;

namespace csb
{

    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Вдудь 3.1.10_1");
            //BotManager manager = new BotManager();
            //manager.Start();

            try
            {

                Test t = new Test();
                t.Start();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            string text = "";
            do
            {
                text = Console.ReadLine();
            } while (!text.Equals("quit"));

        }
    }

    class Test
    {
        public Test() { }

        public int[] data = new int[50];
        public void Start()
        {


            //Task[] tasks = new Task[20];
            //for (int i = 0; i < tasks.Length; i++)
            //    tasks[i] = test(i, 500);

            Test1[] t_1000 = new Test1[20];
            for (int i = 0; i < t_1000.Length; i++)
                t_1000[i] = new Test1($"{i}".PadLeft(3, ' '), 1000);

            //Test1[] t_7000 = new Test1[20];
            //for (int i = 0; i < t_7000.Length; i++)
            //    t_7000[i] = new Test1($"{i}".PadLeft(3, ' '), 7000);

            //Test2[] tests = new Test2[50];
            //for (int i = 0; i < tests.Length; i++)
            //{
            //    tests[i] = new Test2($"{i}", 0);
            //}

            //var t1 = test(1, 50);
            //var t2 = test(2, 50);
            //var t3 = test(3, 50);
        }

        Task test(int name, int n)
        {

            //TGStatApi api = new TGStatApi("http://185.46.9.229:4000");
            TGStatApi api = new TGStatApi("http://136.243.74.153:4000");

            return Task.Run(async () => {

                //var subs = await api.GetUsersNeedDailyPush("INDA01", 24);
                var subs = await api.GetUsersNeedDailyPush("INDA01", 24);
                Console.WriteLine($"{subs.Count}");

                for (int i = 0; i < n; i++)
                {

                    try
                    {
                        await api.MarkFollowerWasDailyPushed("INDA01", 1481806946, 8, TGFollowersStatApi.DailyPushState.delivered);
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    Console.WriteLine($"{name} {i}");
                }

            });
        }

    }

    class Test1
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        TGStatApi api = new TGStatApi("http://136.243.74.153:4000");
        string name;
        public Test1(string name, int interval)
        {
            this.name = name;
            timer.Interval = interval;
            timer.AutoReset = false;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            Console.WriteLine(DateTime.Now);

            //var subs = await api.GetUsersNeedDailyPush("INDA01", 24);
            //Console.WriteLine($"{subs.Count}");

            //int count = subs.Count;
            int count = 1300;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    await api.MarkFollowerWasDailyPushed("INDA01", 1481806946, 8, TGFollowersStatApi.DailyPushState.delivered);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine($"{name} {i}");
            }

            Console.WriteLine(DateTime.Now);

        }
    }

    class Test2
    {

        Timer timer;
        TGStatApi api = new TGStatApi("http://136.243.74.153:4000");
        string name;
        public Test2(string name, int interval)
        {
            this.name = name;
            timer = new Timer(new TimerCallback(timerCallBack), null, 0, 60000);

        }

        int cntr = 0;
        async void timerCallBack(object state)
        {
            var subs = await api.GetUsersNeedDailyPush("INDA01", 24);
            Console.WriteLine($"{cntr++} {subs.Count}");

            foreach (var item in subs)
            {
                try
                {
                    await api.MarkFollowerWasDailyPushed("INDA01", 1481806946, 8, TGFollowersStatApi.DailyPushState.delivered);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine($"{name} {item.tg_user_id}");
            }
        }


    }
}
