using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HackMaineIrcBot.Irc.Hooks;

namespace HackMaineIrcBotSvc_Tests
{
    [TestClass]
    public class UrlHookTests
    {
        [TestMethod]
        public void TestCanHandle()
        {
            string urlOK1 = "http://hackmaine.org";
            string urlOK1u = "HTtp://HACKMAINE.org";
            string urlOK2 = "http://hackmaine.org/forums";
            string urlOK3 = "http://hackmaine.org/forums/";
            string urlOK4 = "http://hackmaine.org/forums?hello=there&id=55";
            string urlOK5 = "https://msdn.microsoft.com/en-us/library/ms563775(v=office.14).aspx";

            string MessageOK1 = "Guys, check out http://hackmaine.org/forums/, if you dare";

            string urlBad1 = "ht://hello.org";
            string urlBad2 = "http://hello";
            string urlBad3 = "http://hackmaine. org/forums/";
            string urlBad4 = "http://hackmaine/forums.org/";

            var urlHook = new UrlHook();

            Assert.IsTrue(urlHook.CanHandle(urlOK1));
            Assert.IsTrue(urlHook.CanHandle(urlOK1u));
            Assert.IsTrue(urlHook.CanHandle(urlOK2));
            Assert.IsTrue(urlHook.CanHandle(urlOK3));
            Assert.IsTrue(urlHook.CanHandle(urlOK4));
            Assert.IsTrue(urlHook.CanHandle(urlOK5));

            Assert.IsTrue(urlHook.CanHandle(MessageOK1));

            Assert.IsFalse(urlHook.CanHandle(urlBad1));
            Assert.IsFalse(urlHook.CanHandle(urlBad2));
            Assert.IsFalse(urlHook.CanHandle(urlBad3));
            Assert.IsFalse(urlHook.CanHandle(urlBad4));
        }

        [TestMethod]
        public void GetTileTest()
        {
            string title = "Page Title";
            var html = string.Format("<html><head><title>{0}</title></head><body>Body text</body></html>", title);
            var doc = UrlHook.ParseHtml(html);

            var expected = title;
            var actual = UrlHook.GetDocumentTitle(doc);

            Assert.AreEqual(expected, actual);
        }

    }
}
