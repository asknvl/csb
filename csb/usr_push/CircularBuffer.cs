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
        Queue<long> queue;
        int capacity;
        #endregion
        public CircularBuffer(int capacity) { 
            this.capacity = capacity;
            queue = new Queue<long>(capacity);
        }

        #region public
        public void Add(long id)
        {
            if (queue.Count == capacity)
                queue.Dequeue();

            queue.Enqueue(id);
        }

        public bool ContainsID(long id)
        {
            return queue.Contains(id);
        }

        public override string ToString()
        {
            string res = "";
            if (queue.Count == 0)
                return "Buffer is empty";

            foreach (var item in queue)
                res += $"{item}\n";

            return res;
        }
        #endregion
    }
}
