using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public string[] ContentTypes { get { return _attr.ContentTypes; } }
        public Regex MatchRegEx { get { return _attr.MatchRegEx; } }
        public string[] MatchDomainNames { get { return _attr.MatchDomainNames; } }

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

        internal static async Task<string> GetPageDescription(string url)
        {
            System.Net.WebResponse response;

            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.CreateHttp(url);
                response = await request.GetResponseAsync();
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

            UrlResolver handler = GetHttpResponseHandler(response);
            if (handler == null)
                return null;

            return await handler.Handle(response);
        }

        internal static UrlResolver GetHttpResponseHandler(System.Net.WebResponse response)
        {
            foreach (var handler in HandlersForDomain(response.ResponseUri.Host))
                if (handler.CanHandle(response))
                    return handler;

            foreach (var handler in HandlersByRegex(response.ResponseUri).Where(m=>m.MatchDomainNames==null))
                if (handler.CanHandle(response))
                    return handler;

            foreach (var handler in HandlersForType(response.ContentType).Where(m => m.MatchDomainNames == null && m.MatchRegEx==null))
                if(handler.CanHandle(response))
                    return handler;

            return null;
        }
    
        private static IEnumerable<UrlResolver> HandlersForDomain(string p)
        {
            return Registry.Where(m => m.MatchDomainNames != null && m.IsMatchDomain(p));
        }

        private static IEnumerable<UrlResolver> HandlersByRegex(Uri uri)
        {
            return Registry.Where(m => m.MatchRegEx != null && m.IsMatchRegEx(uri.ToString()));
        }

        private static IEnumerable<UrlResolver> HandlersForType(string contenttype)
        {
            return Registry.Where(m => m.ContentTypes.Any(c => contenttype.StartsWith(c)));
        }

        private bool CanHandle(System.Net.WebResponse response)
        {
            Uri uri=response.ResponseUri;
            return
                (ContentTypes.Any(c => response.ContentType.StartsWith(c))) &&
                (MatchDomainNames == null || IsMatchDomain(uri.Host)) &&
                (MatchRegEx == null || IsMatchRegEx(uri.ToString()));
        }

        private bool IsMatchRegEx(string url)
        {
            return MatchRegEx.IsMatch(url);
        }

        private bool IsMatchDomain(string host)
        {
            return MatchDomainNames.Any(n => Insensitive.Equals(host, n));
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
        public Regex MatchRegEx { get; set; }
        public string[] MatchDomainNames { get; set; }
        public string[] ContentTypes { get; set; }

        public URLResolverAttribute(string MatchDomainNames = null, string MatchRegEx = null, string ContentTypes="text/html;", string Name = null, string Description = null)
        {
            this.MatchDomainNames = MatchDomainNames == null ? null : MatchDomainNames.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            this.MatchRegEx = MatchRegEx == null ? null : new Regex(MatchRegEx);
            this.ContentTypes = ContentTypes.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            this.Name = Name ?? this.GetType().Name;
            this.Description = Description ?? "Undescribed";
        }
    }

}
