﻿using System.Threading.Tasks;

namespace capi_test.capi
{
    public interface ICAPI
    {
        public Task<string> MakeLeadEvent(string pixel_id,
                                  string token,
                                  long? tg_user_id = null,
                                  string firstname = null,
                                  string lastname = null,
                                  string client_user_agent = null,
                                  string client_ip_address = null,
                                  string fbc = null,
                                  string fbp = null,
                                  string test_event_code = null);
        public Task<string> MakeContactEvent(string pixel_id,
                                  string token,
                                  long? tg_user_id = null,
                                  string firstname = null,
                                  string lastname = null,
                                  string client_user_agent = null,
                                  string client_ip_address = null,
                                  string fbc = null,
                                  string fbp = null,
                                  string test_event_code = null);
    }
}
