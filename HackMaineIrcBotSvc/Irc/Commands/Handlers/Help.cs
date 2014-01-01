using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Commands
{
    [IRCCommand("help")]
    class Help : BaseIrcCommand
    {
        public override string Description { get { return "Provides a list of commands"; } }

        public override void OnChannelCommand(string channel, string args)
        {
            foreach (var nfo in IrcCommandHandler.Registry)
            {
                SendChannelMessage(channel, "{0}: {1}", nfo.Key, nfo.Value.Description);
            }
        }
    }
}
