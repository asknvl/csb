using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_poster
{
    public interface IUpdateObserver
    {
        void Update(TL.IObject update);
    }
}
