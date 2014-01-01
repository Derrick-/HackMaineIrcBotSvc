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
        public override string UsageSuffix { get { return "[commands|hooks|all][verbose]"; } }

        public override void OnChannelCommand(string channel, string command, IEnumerable<string> args)
        {
            bool verbose=false;
            foreach(var arg in args)
                if(Insensitive.Equals(arg,"verbose"))
                {
                    verbose=true;
                    args = args.Where(m => m != arg);
                    break;
                }

            int argsCount = args.Count();
            if (argsCount > 1)
                SendChannelMessage(channel, "One thing at a time please.");
            else
            {
                string arg;
                if (argsCount == 1)
                    arg = args.First().ToLowerInvariant();
                else
                    arg = "all";
                switch (arg)
                {
                    case "command":
                    case "commands":
                        SendCommandList(channel, verbose);
                        break;
                    case "hook":
                    case "hooks":
                        SendHooksList(channel, verbose);
                        break;
                    case "all":
                        SendCommandList(channel, verbose);
                        SendHooksList(channel, verbose);
                        break;
                    default:
                        KeyValuePair<string, BaseIrcCommand> nfo = IrcCommandHandler.Registry.Where(m => m.Key == arg).FirstOrDefault();
                        if (nfo.Key!=null)
                            SendCommandInfo(channel, nfo, true);
                        else
                        SendChannelMessage(channel, "I can't help you with that.");
                        break;
                }
            }
        }

        private void SendHooksList(string channel, bool verbose = false)
        {
            SendChannelMessage(channel, "There are no registered hooks.");
        }

        private void SendCommandList(string channel, bool verbose = false)
        {
            var items = IrcCommandHandler.Registry;
            SendChannelMessage(channel, "There are {0} registered commands:", items.Count());
            foreach (var nfo in items)
                SendCommandInfo(channel, nfo, verbose);
        }

        private void SendCommandInfo(string channel, KeyValuePair<string, BaseIrcCommand> nfo, bool verbose = false)
        {
            if (verbose)
                SendChannelMessage(channel, "{0,40}: {1}", string.Format("{0} {1} ", nfo.Key, nfo.Value.UsageSuffix), nfo.Value.Description);
            else
                SendChannelMessage(channel, "{0,10}: {1}", nfo.Key, nfo.Value.Description);
        }
    }
}
