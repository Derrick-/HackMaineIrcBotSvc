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
        
        protected abstract void Handle(CommandResponder responder, string command, IEnumerable<string> args);

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

        protected class QueryResponder : CommandResponder
        {
            readonly string user;
            public QueryResponder(string user)
            {
                this.user = user;
            }

            public override void SendResponseLine(string format, params object[] args)
            {
                SendMessage(SendType.Message, user, format, args);
            }
        }

        protected class ChannelResponder : CommandResponder
        {
            readonly string channel;
            public ChannelResponder(string channel)
            {
                this.channel = channel;
            }

            public override void SendResponseLine(string format, params object[] args)
            {
                SendChannelMessage(channel, format, args);
            }
        }

        protected abstract class CommandResponder
        {
            public abstract void SendResponseLine(string format, params object[] args);

            protected void SendChannelMessage(string channel, string format, params object[] args)
            {
                SendMessage(SendType.Message, channel, string.Format(format, args));
            }

            protected void SendMessage(SendType type, string destination, string message)
            {
                IrcBot.SendMessage(type, destination, message);
            }
 
            protected void SendMessage(SendType type, string destination, string format, params object[] args)
            {
                IrcBot.SendMessage(type, destination, string.Format(format, args));
            }
        }

    }
}
