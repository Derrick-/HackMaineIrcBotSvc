using HackMaineIrcBot.Irc.Hooks;
using System.Collections.Generic;
using System.Linq;

namespace HackMaineIrcBot.Irc.Commands.Handlers
{
    [IRCCommand("help")]
    class Help : BaseIrcCommand
    {
        public override string Description { get { return "Provides a list of functions"; } }
        public override string UsageSuffix { get { return "[{command_name}|commands|hooks|all][verbose]"; } }

        protected override void Handle(BaseIrcResponder responder, string command, IEnumerable<string> args)
        {
            bool verbose = false;
            foreach(var arg in args)
                if(Insensitive.Equals(arg,"verbose"))
                {
                    verbose=true;
                    args = args.Where(m => m != arg);
                    break;
                }

            int argsCount = args.Count();
            if (argsCount > 1)
                responder.SendResponseLine("One thing at a time please.");
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
                        SendCommandList(responder, verbose);
                        break;
                    case "hook":
                    case "hooks":
                        SendHooksList(responder, verbose);
                        break;
                    case "all":
                        SendCommandList(responder, verbose);
                        SendHooksList(responder, verbose);
                        break;
                    default:
                        KeyValuePair<string, BaseIrcCommand> nfo = IrcCommandHandler.Registry.Where(m => m.Key == arg).FirstOrDefault();
                        if (nfo.Key != null)
                        {
                            responder.SendResponseLine("Command {0}{1}:", IrcCommandHandler.commandPrefix, nfo.Key);
                            SendCommandInfo(responder, nfo, true);
                        }
                        else
                            responder.SendResponseLine("I can't help you with that.");
                        break;
                }
            }
        }

        private void SendCommandList(BaseIrcResponder responder, bool verbose = false)
        {
            var items = IrcCommandHandler.Registry;
            responder.SendResponseLine("There are {0} registered commands:", items.Count());
            foreach (var nfo in items)
                SendCommandInfo(responder, nfo, verbose);
        }

        private void SendHooksList(BaseIrcResponder responder, bool verbose = false)
        {
            var items = BaseIrcHook.Registry;
            int count = items.Count();
            if (count < 1)
                responder.SendResponseLine("There are no registered hooks.");
            else
            {
                responder.SendResponseLine("There are {0} registered hooks:", count);
                foreach (var hook in items)
                    SendHookInfo(responder, hook, verbose);
            }
        }

        private void SendCommandInfo(BaseIrcResponder responder, KeyValuePair<string, BaseIrcCommand> nfo, bool verbose = false)
        {
            if (verbose)
                responder.SendResponseLine("  {0}{1,-40}: {2}", IrcCommandHandler.commandPrefix, string.IsNullOrWhiteSpace(nfo.Value.UsageSuffix) ? nfo.Key : string.Format("{0} {1}", nfo.Key, nfo.Value.UsageSuffix), nfo.Value.Description);
            else
                responder.SendResponseLine("  {0}{1,-10}: {2}", IrcCommandHandler.commandPrefix, nfo.Key, nfo.Value.Description);
        }

        private void SendHookInfo(BaseIrcResponder responder, BaseIrcHook hook, bool verbose)
        {
            responder.SendResponseLine("  {0,-10}: {1}", hook.Name, hook.Description);
        }
    }
}
