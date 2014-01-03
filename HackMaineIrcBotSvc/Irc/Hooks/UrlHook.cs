using HtmlAgilityPack;
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

        protected override async Task<bool> Handle(BaseIrcResponder responder, ChannelMessageEventArgs e)
        {
            IEnumerable<string> found;
            if (FindUrl(e.Data.Message, out found))
            {
                bool handledAny = false;
                foreach (var url in found)
                {
                    string response = await GetPageTitle(url);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        responder.SendResponseLine(" [{0}]", response);
                        handledAny = true;
                    }
                }
                return handledAny;
            }
            return false;
        }

        private async Task<string> GetPageTitle(string url)
        {
            string html;
            try
            {
                html = await GetHtml(url);
            }
            catch (System.Net.WebException ex)
            {
                return ex.Message;
            }
            catch (ArgumentException ex)
            {
                return ex.Message;
            }
            catch (FormatException ex)
            {
                return ex.Message;
            }
            var doc = ParseHtml(html);
            return GetDocumentTitle(doc);
        }

        private async Task<string> GetHtml(string url)
        {
            System.Net.HttpWebRequest request = null;
            System.Net.WebResponse response = null;
            request = System.Net.HttpWebRequest.CreateHttp(url);
            response = await request.GetResponseAsync();

            var header = response.Headers;
            if (header == null || !((string)header["Content-Type"]).StartsWith("text/html"))
                return null;

            string html;
            using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                html = await sr.ReadToEndAsync();

            return html;
        }

        internal static string GetDocumentTitle(HtmlDocument doc)
        {
            var title = doc.DocumentNode.SelectNodes("/html/head/title");
            if (title == null)
                return null;
            var titleLine = title.First().InnerText.Trim().Split(new char[] { '\n', '\r' }, 1, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return titleLine.Trim();
        }

        internal static HtmlDocument ParseHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
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
