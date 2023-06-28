using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.logger
{
    public class Logger : ILogger
    {
        #region const
        string logFolder = "logs";
        #endregion

        #region vars
        Queue<LogMessage> logMessages = new Queue<LogMessage>();
        System.Timers.Timer timer = new System.Timers.Timer();
        string filePath;
        string TAG;
        #endregion

        #region properties
        bool disableFileOutput;
        public bool DisableFileOutput
        {
            get => disableFileOutput;
            set
            {
                if (value)
                    timer.Stop();
                else
                    timer.Start();

                disableFileOutput = value;
            }
        }

        public bool EnableConsoleOutput { get; set; }
        public bool EnableConsoleErrorOutput { get; set; }
        #endregion        

        public Logger(string tag, string foldername, string filename)
        {

            TAG = tag;

#if DEBUG
            EnableConsoleOutput = true;
#else
            EnableConsoleOutput= false;
            EnableConsoleErrorOutput= true;
#endif
            var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), logFolder, foldername);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            filePath = Path.Combine(fileDirectory, $"{filename}.log");

            if (File.Exists(filePath))
                File.Delete(filePath);

            timer.Interval = 1000;
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            appendLogFile();
        }

        #region private
        void appendLogFile()
        {
            try
            {

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    while (logMessages.Count > 0)
                    {
                        LogMessage message = logMessages.Dequeue();
                        if (message != null)
                            sw.WriteLine(message.ToString());
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region public
        public void dbg(string text)
        {
            var message = new LogMessage(LogMessageType.dbg, TAG, text);
            if (EnableConsoleOutput)
                Console.WriteLine(message.ToString());

            if (!DisableFileOutput)
                logMessages.Enqueue(message);
        }

        public void err(string text)
        {
            var message = new LogMessage(LogMessageType.err, TAG, text);
            if (EnableConsoleOutput || EnableConsoleErrorOutput)
                Console.WriteLine(message.ToString());

            if (!DisableFileOutput)
                logMessages.Enqueue(message);
        }

        public void inf(string text)
        {
            var message = new LogMessage(LogMessageType.inf, TAG, text);
            if (EnableConsoleOutput)
                Console.WriteLine(message.ToString());

            if (!DisableFileOutput)
                logMessages.Enqueue(message);
        }

        public void inf_urgent(string text)
        {
            var message = new LogMessage(LogMessageType.inf, TAG, text);            
                Console.WriteLine(message.ToString());

            if (!DisableFileOutput)
                logMessages.Enqueue(message);
        }
        #endregion
    }
}
