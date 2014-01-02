using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Commands.Handlers
{
    [IRCCommand("version")]
    class Version : BaseIrcCommand
    {
        public override string Description { get { return "Displays software version"; } }

        protected override void Handle(CommandResponder responder, string command, IEnumerable<string> args)
        {
            responder.SendResponseLine("{0} v{1}.{2}.{3}.{4}", Program.Name, Program.Version.Major, Program.Version.Minor, Program.Version.Revision, Program.Version.Build);
        }
    }
}
