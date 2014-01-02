using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks
{
    [IRCHook("URL Helper","Fetches information on URL's")]
    [TypePriority(MemberPriority.Lowest)]
    class UrlHook : BaseIrcHook
    {
        const string urlMatcher = @"(https?)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-\(\)]+))*";
        Regex urlEx = new Regex(@"(?nix).*(?<URL>\b(" + urlMatcher + "))+.*");

        protected override bool Handle(ChannelMessageEventArgs e)
        {
            return CanHandle(e.Data.Message);
        }

        internal bool CanHandle(string message)
        {
            string found;
            return CanHandle(message, out found);
        }

        internal bool CanHandle(string message, out string found)
        {
            found=null;
            var match = urlEx.Match(message);
            if(!match.Success) return false;
            var group = match.Groups["URL"];
            found = group.Captures[0].Value;
            return true;
        }
    }
}
