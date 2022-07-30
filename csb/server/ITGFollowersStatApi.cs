using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.server
{
    public interface ITGFollowersStatApi
    {       
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
