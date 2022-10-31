using csb.logger;
using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TL;

namespace csb.usr_push
{
    public class UserAdmin : TGUserBase
    {
        #region vars
        settings.GlobalSettings globals = settings.GlobalSettings.getInstance();
#if DEBUG
        protected TGStatApi statApi = new TGStatApi("http://185.46.9.229:4000");
#else
        //TGStatApi statApi = new TGStatApi("http://136.243.74.153:4000");
        protected TGStatApi statApi = new TGStatApi("http://192.168.72.51:4000");

#endif

        ILogger log = Logger.getInstance();
#endregion

#region properties        
#endregion

        public UserAdmin(string api_id, string api_hash, string phone_number, string geotag) : base(api_id, api_hash, phone_number, geotag)
        {
        }

#region protected
        protected override async void processUpdate(Update update)
        {
            try
            {
                switch (update)
                {
                    case UpdateNewMessage unm:
                        long id = unm.message.Peer.ID;
                        await statApi.MarkFollowerMadeFeedback(geotag, id);
                        Console.WriteLine($"{DateTime.Now} FEEDBACK on {geotag} from {id}");                        
                        break;
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
#endregion

#region public        
#endregion
    }
}
