using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks.UrlResolvers
{
    [TypePriority(MemberPriority.Lowest)]
    [URLResolver(Name:"Title Resolver",Description:"Returns the html Title of the URL if available.")]
    class TitleResolver : UrlResolver
    {
        public override async Task<string> Handle(System.Net.WebResponse response)
        {
            string html = await GetHtml(response);
            var doc = ParseHtml(html);
            return GetDocumentTitle(doc);
        }
    }
}
