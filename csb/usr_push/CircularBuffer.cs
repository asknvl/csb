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
        List<long> buffer = new();
        int capacity;
        uint count = 0;
        #endregion
        public CircularBuffer(int capacity) {             
            this.capacity = capacity;
        }

        #region public
        public void Add(long id)
        {
            if (!buffer.Contains(id))
            {
                count++;
                buffer.Add(id);
                if (buffer.Count > capacity)
                    buffer.RemoveAt(0);
            }            
        }

        public bool ContainsID(long id)
        {
            return buffer.Contains(id);
        }

        public override string ToString()
        {
            return $"Circular buffer: cntr={count} length={buffer.Count}";
        }
        #endregion
    }
}
