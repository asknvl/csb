using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.usr_push
{
    public class CircularBuffer
    {
        #region vars
        long[] buffer;        
        int cntr = 0;
        object l = new object();
        #endregion
        public CircularBuffer(int capacity) {             
            buffer = new long[capacity];
        }

        #region public
        public void Add(long id)
        {
            lock (l)
            {
                buffer[cntr] = id;
                cntr++;
                cntr %= buffer.Length;
            }
        }

        public bool ContainsID(long id)
        {
            lock (l)
            {
                return buffer.Contains(id);
            }
        }

        public override string ToString()
        {
            return $"Circular buffer: cntr={cntr} length={buffer.Length}";
        }
        #endregion
    }
}
