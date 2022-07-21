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
        private const int store_time = 1440; 
        #endregion

        #region vars
        Queue<string[]> messageQueue;
        #endregion

        #region properties
        #endregion

        public TextMatchingAnalyzer(int period)
        {

        }

        #region public
        public void Add(string message)
        {
            var splt = message.Split(" ");
            //messageList.Add(splt);
        }

        public bool Check(string message)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
