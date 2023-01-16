using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace capi_test.capi
{
    public interface ICAPI
    {   
        public Task MakeLeadEvent(string pixel_id, string token, long tg_user_id, string firstname = null, string lastname = null, string client_user_agent = null, string client_ip_address = null, string test_event_code = null);
    }
}
