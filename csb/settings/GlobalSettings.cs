using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.settings
{
    public class GlobalSettings
    {
        #region properties
        public string listener_api_id { get; set; }
        public string listener_api_hash { get; set; }
        public string push_api_id { get; set; }
        public string push_api_hash { get; set; }
        #endregion

        static GlobalSettings instance;
        private GlobalSettings() {
#if VESTNIK || LATAM
            listener_api_id = "16532988";
            listener_api_hash = "05a55aa70deae546f5eb4b2892a56606";
#endif
#if DEBUG
            listener_api_id = "13180345";
            listener_api_hash = "df78e4859fb0cbd03dc5cf83d5d0c0cb";
#endif
            push_api_id = "23400467";
            push_api_hash = "af8d86630f308931e5bcbdf045724f7c";
        }
        public static GlobalSettings getInstance()
        {
            if (instance == null)
                instance = new GlobalSettings();
            return instance;
        }
    }
}
