using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc
{
    public static class IrcEvents
    {
        public delegate void OnQueryEventHandler(QueryEventArgs e);
        public delegate void OnChannelMessageEventHandler(ChannelMessageEventArgs e);

        public static event OnQueryEventHandler OnQuery;
        public static event OnChannelMessageEventHandler OnChannelMessage;

        public static void InvokeOnQuery(QueryEventArgs e)
        {
            if (OnQuery != null)
                OnQuery(e);
        }

        public static void InvokeOnChannelMessage(ChannelMessageEventArgs e)
        {
            if (OnChannelMessage != null)
                OnChannelMessage(e);
        }
    }

    public class ChannelMessageEventArgs : IrcEventArgs
    {
        internal ChannelMessageEventArgs(IrcMessageData data) : base(data) { }
    }
    
    public class QueryEventArgs : IrcEventArgs
    {
        internal QueryEventArgs(IrcMessageData data) : base(data) { }
    }
}
