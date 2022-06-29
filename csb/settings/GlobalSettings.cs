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
        public string api_id { get; set; }
        public string api_hash { get; set; }
        #endregion

        static GlobalSettings instance;
        private GlobalSettings() {
            api_id = "17730462";
            api_hash = "10f1c6f26189d6c95f5fbeefe635d115";
        }
        public static GlobalSettings getInstance()
        {
            if (instance == null)
                instance = new GlobalSettings();
            return instance;
        }
    }
}
