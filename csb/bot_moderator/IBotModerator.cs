using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    internal interface IBotModerator
    {
        string GeoTag { get; set; }
        string Name { get; protected set; }
        string Token { get; set; }
        bool IsRunning { get; protected set; }
        void Start();
        void Stop();
    }
}
