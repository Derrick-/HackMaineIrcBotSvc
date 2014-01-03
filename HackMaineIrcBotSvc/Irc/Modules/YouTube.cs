using HackMaineIrcBot.Irc.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HackMaineIrcBot.Irc.Hooks.UrlResolvers
{
    [TypePriority(MemberPriority.Normal)]
    [URLResolver(
        MatchDomainNames: YouTube.DomainList,
        MatchRegEx: YouTube.VideoRexEx,
        Name: "YouTube Resolver", Description: "Returns YouTube video details.")]
    class YouTubeResolver : UrlResolver
    {
        public override async Task<string> Handle(System.Net.WebResponse response)
        {
            string vidid = YouTube.GetVideoID(response.ResponseUri.ToString());
            return string.Format("[YouTube] {0}", await YouTube.GetVideoInfo(vidid));
        }
    }
}

namespace HackMaineIrcBot.Irc.Modules
{
    static class YouTube
    {
        public class VideoInfo
        {
            public VideoInfo(dynamic jObject)
            {
                dynamic entry = ((dynamic)((Newtonsoft.Json.Linq.JObject)(jObject))).entry;
                try
                {
                    ID = ((string)entry["id"]["$t"]).Split(':').Last();
                }
                catch
                {
                    ID = null;
                    return;
                }

                Link = string.Format(YouTube.formatVideoLink, ID);

                try
                {
                    Title = entry.title["$t"];
                    Uploader = entry["author"][0]["name"]["$t"];
                    Uploaded = entry["published"]["$t"];
                    int seconds = entry["media$group"]["yt$duration"]["seconds"];
                    Duration = TimeSpan.FromSeconds(seconds);
                    Views = entry["yt$statistics"]["viewCount"];
                    Comments = entry["gd$comments"]["gd$feedLink"]["countHint"];
                    Likes = entry["yt$rating"]["numLikes"];
                    Dislikes = entry["yt$rating"]["numDislikes"];
                }
                catch { }
            }

            public string ID { get; set; }
            public string Link { get; set; }
            public string Title { get; set; }
            public string Uploader { get; set; }
            public DateTime? Uploaded { get; set; }
            public TimeSpan? Duration { get; set; }
            public int? Views { get; set; }
            public int? Comments { get; set; }
            public int? Likes { get; set; }
            public int? Dislikes { get; set; }

            public string UploadedFormated
            {
                get
                {
                    if (Uploaded.HasValue)
                    {
                        if ((DateTime.Now - Uploaded.Value).TotalHours < 24)
                            return Uploaded.Value.ToShortTimeString();
                        return Uploaded.Value.ToShortDateString();
                    }
                    return "N/A";
                }
            }

            public string Description
            {
                get
                {
                    if (ID == null) return "API_ERROR";
                    return "Title: " + (Title ?? "API ERROR") +
                     " | Uploader: " + (Uploader ?? "N/A") +
                     " | Uploaded: " + UploadedFormated +
                     " | Duration: " + (Duration.ToString() ?? "N/A") +
                     " | Views: " + (Views.HasValue ? Views.ToString() : "N/A") +
                     " | Comments: " + (Comments.HasValue ? Comments.ToString() : "N/A") +
                     " | Likes: " + (Likes.HasValue ? Likes.ToString() : "N/A") +
                     " | Dislikes: " + (Dislikes.HasValue ? Dislikes.ToString() : "N/A") +
                     " | Link: " + Link;
                }
            }

        }

        public const string DomainList = "youtube.com;www.youtube.com;youtu.be";
        public const string VideoRexEx = @"(?nix)(https?\://)((www.)?youtube.com/watch\S*v=|youtu.be/)(?<ID>([\w-]+))";
        public static string formatVideoLink = "http://youtu.be/{0}";
        public const string formatApiLink = "http://gdata.youtube.com/feeds/api/videos/{0}?v=2&alt=json";

        internal static Regex VideoMatcher = new Regex(VideoRexEx);

        internal static string GetVideoID(string url)
        {
            var match = VideoMatcher.Match(url);
            string vidid = match.Groups["ID"].Value;
            return vidid;
        }

        internal async static Task<string> GetVideoInfo(string vidid)
        {
            System.Net.WebResponse response;
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.CreateHttp(string.Format(formatApiLink, vidid));
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

            string json;
            using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                json = await sr.ReadToEndAsync();

            var entry = new VideoInfo(JsonConvert.DeserializeObject(json));
            return entry.Description;
        }
    }
}
