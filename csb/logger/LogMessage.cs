﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.logger
{
    public enum LogMessageType
    {
        dbg,
        err,
        inf,
    }
    public class LogMessage
    {
        public LogMessageType Type { get; }
        public string Text { get; }
        public string Date { get; }

        public LogMessage(LogMessageType type, string text) { 
            Type = type;
            Text = text;
            Date = DateTime.Now.ToString();
        }

        public override string ToString()
        {
            return $"{Date} {Type} > {Text}";
        }
    }

}