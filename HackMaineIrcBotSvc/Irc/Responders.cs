using Meebey.SmartIrc4net;

namespace HackMaineIrcBot.Irc
{
    internal class QueryResponder : BaseIrcResponder
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

    internal class ChannelResponder : BaseIrcResponder
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

    internal abstract class BaseIrcResponder
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
