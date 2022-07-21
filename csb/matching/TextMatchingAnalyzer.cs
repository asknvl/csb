using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.matching
{
    public class TextMatchingAnalyzer : ITextMatchingAnalyzer
    {

        #region const
        string[] separators = new string[] { " ", ",", ".", "\n"};        
        #endregion

        #region vars
        Queue<string[]> messageQueue;        
        #endregion

        #region properties
        public int Capacity { get; set; }
        public int Percentage { get; set; }
        #endregion

        public TextMatchingAnalyzer(int capacity)
        {
            Capacity = capacity;
            messageQueue = new Queue<string[]>();
        }

        #region public
        public void Add(string message)
        {
            if (messageQueue.Count >= Capacity)
                messageQueue.Dequeue();
            string[] splt = message.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            messageQueue.Enqueue(splt);
        }

        public int Check(string text)
        {
            var splt = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            int length = splt.Length;

            foreach (var message in messageQueue)
            {

                var tmp = new List<string>(splt);
                int counter = 0;

                foreach (var word in message)
                {
                    if (tmp.Contains(word))
                        counter++;
                }
                Console.WriteLine(counter);
            }

            return 0;
        }
        #endregion
    }
}
