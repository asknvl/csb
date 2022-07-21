using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.bot_moderator
{
    public interface IBotModerator
    {
        string GeoTag { get; set; }
        string Name { get; set; }
        string Token { get; set; }
        bool IsRunning { get; set; }
        void Start();
        void Stop();
    }
}
