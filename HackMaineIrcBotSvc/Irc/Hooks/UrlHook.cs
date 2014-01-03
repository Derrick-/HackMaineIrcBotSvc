using HackMaineIrcBot.Irc.Hooks.UrlResolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks
{
    [IRCHook("URL Helper", "Fetches information on URL's")]
    [TypePriority(MemberPriority.Lowest)]
    class UrlHook : BaseIrcHook
    {
        const string urlMatcher = @"(https?)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2}))(\:[0-9]+)?(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-\(\)]+))*";
        Regex urlEx = new Regex(@"(?nix)(?<URL>\b(" + urlMatcher + "))+");

        struct UrlCacheEntry
        {

            public UrlCacheEntry(string description)
            {
                Description = description;
                Created = DateTime.Now;
            }

            public string Description;
            public DateTime Created;
            public bool IsExpired { get { return DateTime.Now - Created < UrlHook.CacheExpireTime; } }

        }

        public static TimeSpan CacheExpireTime = TimeSpan.FromMinutes(1.0);
        private static Dictionary<string, UrlCacheEntry> UrlCache = new Dictionary<string, UrlCacheEntry>();

        protected override async Task<bool> Handle(BaseIrcResponder responder, ChannelMessageEventArgs e)
        {
            IEnumerable<string> found;
            if (FindUrl(e.Data.Message, out found))
            {
                bool handledAny = false;
                foreach (var url in found.Distinct())
                {
                    string description;
                    UrlCacheEntry entry;
                    if (UrlCache.TryGetValue(url, out entry) && !entry.IsExpired)
                        description = entry.Description;
                    else
                    {
                        description = await UrlResolver.GetPageDescription(url);
                        UrlCache[url] = new UrlCacheEntry(description);
                    }
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        responder.SendResponseLine(" {0}", description);
                        handledAny = true;
                    }
                }
                return handledAny;
            }
            return false;
        }

        internal bool CanHandle(string message)
        {
            IEnumerable<string> found;
            return FindUrl(message, out found);
        }

        internal bool FindUrl(string message, out IEnumerable<string> found)
        {
            found = null;
            var matches = urlEx.Matches(message);
            if (matches.Count < 1) return false;

            found = new List<string>(matches.Cast<Match>().Select(m => m.Value));
            return true;
        }
    }
}
