﻿using System;
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
            api_id = "16532988";
            api_hash = "05a55aa70deae546f5eb4b2892a56606";
#endif
#if DEBUG
            listener_api_id = "13180345";
            listener_api_hash = "df78e4859fb0cbd03dc5cf83d5d0c0cb";
#endif
        }
        public static GlobalSettings getInstance()
        {
            if (instance == null)
                instance = new GlobalSettings();
            return instance;
        }
    }
}
