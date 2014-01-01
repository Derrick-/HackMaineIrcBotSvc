using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HackMaineIrcBot.Irc.Commands
{
    class IRCCommandAttribute : Attribute
    {
        public string CommandString { get; set; }
        public IRCCommandAttribute(string CommandString)
        {
            this.CommandString = CommandString;
        }
    }
}
