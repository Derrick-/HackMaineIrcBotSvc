﻿using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HackMaineIrcBot.Irc.Commands
{
    abstract class BaseIrcCommand
    {
        public abstract string Description {get;}
        public abstract void OnChannelCommand(string channel, string args);

        protected void SendChannelMessage(string channel, string format, params string[] args)
        {
            SendMessage(SendType.Message, channel, string.Format(format, args));
        }

        protected void SendMessage(SendType type, string destination, string message)
        {
            IrcBot.SendMessage(type, destination, message);
        }

    }
}