using csb.storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.settings
{
    public enum Offices
    {
        DBG = 0,
        KRD = 1,
        MSK = 2
    }

    public class Settings
    {
        [JsonProperty]
        public Offices office { get; set; }
        [JsonProperty]
        public string listener_api_id { get; set; }
        [JsonProperty]
        public string listener_api_hash { get; set; }
        [JsonProperty]
        public string push_api_id { get; set; }
        [JsonProperty]
        public string push_api_hash { get; set; }
        [JsonProperty]
        public string bot_manager_token { get; set; }
    }

    public class GlobalSettings
    {
        #region const
        string settings_file_name = "settings.json";
        #endregion

        #region properties                
        public Offices office { get; set; }
        public string listener_api_id { get; set; }
        public string listener_api_hash { get; set; }
        public string push_api_id { get; set; }
        public string push_api_hash { get; set; }
        public string bot_manager_token { get; set; }
        #endregion

        static GlobalSettings instance;

        private GlobalSettings()
        {

            office = Offices.DBG;

#if RELEASE
            listener_api_id = "16532988";
            listener_api_hash = "05a55aa70deae546f5eb4b2892a56606";
            bot_manager_token = "5597155386:AAEvPn9KUuWRPCECuOTJDHdh6RiY_IVbpWM";
            
#endif
#if DEBUG || CAPI_DEBUG
            listener_api_id = "13180345";
            listener_api_hash = "df78e4859fb0cbd03dc5cf83d5d0c0cb";
            bot_manager_token = "5921412686:AAGZzg0V1enYadLf_5YEycoEQBES8LyXc1A";
#endif
            push_api_id = "23400467";
            push_api_hash = "af8d86630f308931e5bcbdf045724f7c";


            if (File.Exists(settings_file_name))
            {
                var json = File.ReadAllText(settings_file_name);
                var p = JsonConvert.DeserializeObject<Settings>(json);

                office = p.office;
                listener_api_id = p.listener_api_id;
                listener_api_hash = p.listener_api_hash;
                push_api_id = p.push_api_id;
                push_api_hash = p.push_api_hash;
                bot_manager_token = p.bot_manager_token;

            }
            else
            {
                Settings p = new Settings();
                p.office = office;
                p.listener_api_id = listener_api_id;
                p.listener_api_hash = listener_api_hash;
                p.push_api_id = push_api_id;
                p.push_api_hash = push_api_hash;
                p.bot_manager_token = bot_manager_token;
                var json = JsonConvert.SerializeObject(p, Formatting.Indented);
                File.WriteAllText(settings_file_name, json);
            }


        }
        public static GlobalSettings getInstance()
        {
            if (instance == null)
                instance = new GlobalSettings();
            return instance;
        }
    }
}
