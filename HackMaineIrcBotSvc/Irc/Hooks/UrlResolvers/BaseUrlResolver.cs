using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks.UrlResolvers
{
    abstract class UrlResolver
    {
        public static IEnumerable<UrlResolver> Registry { get { return _registry; } }
        static List<UrlResolver> _registry = new List<UrlResolver>();

        private URLResolverAttribute _attr;
  
        delegate Task<string> HttpResponseHandler(System.Net.WebResponse response);

        public string Name { get { return _attr.Name; } }
        public string Description { get { return _attr.Description; } }
        public abstract Task<string> Handle(System.Net.WebResponse response);

        public static void Initialize()
        {
            RegisterAllHandlers();
        }

        private static void RegisterAllHandlers()
        {
            Console.WriteLine("Registering URL Handlers:");
            ReflectionUtils.RegisterObjects<UrlResolver, URLResolverAttribute>(RegisterSingleHandler);
        }

        private static void RegisterSingleHandler(Type t, object[] attrs)
        {
            UrlResolver handlerobject = (UrlResolver)Activator.CreateInstance(t);
            handlerobject._attr = ((URLResolverAttribute)attrs[0]);
            ConsoleUtils.WriteInfo("\t{0}: {1}", handlerobject.Name, handlerobject.Description);
            _registry.Add(handlerobject);
        }

        protected async Task<string> GetHtml(System.Net.WebResponse response)
        {
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
    }

    [AttributeUsage(AttributeTargets.Class)]
    class URLResolverAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string forRegEx { get; set; }
        public string forDomainName { get; set; }
        public string ContentTypes { get; set; }

        public URLResolverAttribute(string MatchDomainName = null, string MatchRegEx = null, string ContentTypes="text/html", string Name = null, string Description = null)
        {
            this.forDomainName = MatchDomainName;
            this.forRegEx = MatchRegEx;
            this.ContentTypes = ContentTypes;
            this.Name = Name ?? this.GetType().Name;
            this.Description = Description ?? "Undescribed";
        }
    }

}
