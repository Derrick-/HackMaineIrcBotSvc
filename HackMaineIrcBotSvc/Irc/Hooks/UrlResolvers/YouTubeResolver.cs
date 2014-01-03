using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks.UrlResolvers
{
    [TypePriority(MemberPriority.Normal)]
    [URLResolver(
        MatchDomainNames:"youtube.com;www.youtube.com;youtu.be",
        MatchRegEx: @"(?nix)(https?\://)((www.)?youtube.com/watch\S*v=|youtu.be/)(?<ID>([\w-]+))",
        Name: "YouTube Resolver", Description: "Returns YouTube video details.")]
    class YouTubeResolver : UrlResolver
    {
        public override async Task<string> Handle(System.Net.WebResponse response)
        {
            var match = MatchRegEx.Match(response.ResponseUri.ToString());
            string vidid = match.Groups["ID"].Value;
            string apiLink = "http://gdata.youtube.com/feeds/api/videos/" + vidid + "?v=2&alt=json";

            string title = GetDocumentTitle(ParseHtml(await GetHtml(response)));

            return "[YouTube]: " + title + " | http://youtu.be/" + vidid;
        }
    }
}
