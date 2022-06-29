using csb.bot_poster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace csb.usr_listener
{
    interface IUpdatesObservable
    {
        void AddObserver(IUpdateObserver o);
        void RemoveObserver(IUpdateObserver o);
        void NotifyObservers(IObject update);
    }
}
