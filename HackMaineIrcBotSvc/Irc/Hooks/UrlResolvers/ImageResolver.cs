using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace HackMaineIrcBot.Irc.Hooks.UrlResolvers
{
    [TypePriority(MemberPriority.Lowest)]
    [URLResolver(ContentTypes:"image/", Name: "Image Resolver", Description: "Displays image information")]
    class ImageResolver : UrlResolver
    {
        public override async Task<string> Handle(System.Net.WebResponse response)
        {
            StringBuilder sb = new StringBuilder("[Image] Size: ");
            sb.Append(Utilities.Util.FormatBytes(response.ContentLength));
            string mimetype = response.ContentType.Split(';').FirstOrDefault();
            if (mimetype != null && mimetype.StartsWith("image/"))
            {
                string type = mimetype.Split('/').Skip(1).FirstOrDefault();
                if (type != null)
                {
                    WebImage image = null;
                    try
                    {
                        image = await CreateWebImage(response);
                        sb.AppendFormat(" | Type: {0} | Dimensions: {1}x{2}", image.ImageFormat, image.Width, image.Height);
                    }
                    catch
                    {
                        sb.AppendFormat(" | Type: {0} | NOT A VALID IMAGE", type);
                    }
                }
                return sb.ToString();
            }
            return null;
        }

        private static async Task<WebImage> CreateWebImage(System.Net.WebResponse response)
        {
            byte[] bytes;
            using (var reader = new BufferedStream(response.GetResponseStream()))
            using (MemoryStream ms = new MemoryStream())
            {
                await reader.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            WebImage image = new WebImage(bytes);
            return image;
        }
    }
}
