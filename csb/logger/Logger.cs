using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.logger
{
    public class Logger : ILogger
    {
        #region singletone
        static Logger instance;
        public static Logger getInstance()
        {
            if (instance == null)
                instance = new Logger();
            return instance;
        }

        public void dbg(string message)
        {
            Console.WriteLine($"dbg > {message}");
        }

        public void err(string message)
        {
            Console.WriteLine($"err > {message}");
        }

        private Logger() { }
        #endregion
    }
}
