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
        #endregion

        #region properties        
        #endregion

        public UserAdmin(string api_id, string api_hash, string phone_number, string geotag) : base(api_id, api_hash, phone_number, geotag)
        {
        }

        #region protected
        protected override void processUpdate(Update update)
        {
            try
            {
                switch (update)
                {
                    case UpdateNewMessage unm:
                        Console.WriteLine(unm.message.Peer.ID);
                        break;
                }
            } catch (Exception ex)
            {

            }
        }
        #endregion

        #region public        
        #endregion
    }
}
