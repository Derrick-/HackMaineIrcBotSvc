using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HackMaineIrcBot.Irc.Commands
{
    abstract class BaseIrcCommand
    {
        public virtual bool HandlesChannelCommands { get { return true; } }
        public virtual bool HandlesQueryCommands { get { return true; } }
        
        public abstract string Description {get;}
        public virtual string UsageSuffix { get { return null; } }
        
        protected abstract void Handle(BaseIrcResponder responder, string command, IEnumerable<string> args);

        public virtual void OnQueryMessage(string from, string command, IEnumerable<string> args)
        {
            if (HandlesQueryCommands)
                Handle(new QueryResponder(from), command, args);
        }

        public virtual void OnChannelCommand(string channel, string command, IEnumerable<string> args)
        {
            if (HandlesChannelCommands)
                Handle(new ChannelResponder(channel), command, args);
        }

    }
}
