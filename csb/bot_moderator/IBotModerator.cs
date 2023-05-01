﻿using asknvl;
using asknvl.leads;
using Newtonsoft.Json;
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
        long? ChannelID { get; set; }
        LeadAlgorithmType? LeadType { get; set; }
        Dictionary<string, int> PseudoLeads { get; set; }        
        GreetingsData Greetings { get; set; }        
        SmartPushData PushData { get; set; }
        DailyPushData DailyPushData { get; set; }
        bool IsRunning { get; set; }
        void Start();
        void Stop();

        public event Action<IBotModerator> ParametersUpdatedEvent;
    }

    
    
}
