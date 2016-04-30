using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwilioFetchBot.ImageGetter
{
    public class GoogleImageGetter
    {
        private List<string> _urls { get; set; }
        public List<string> Urls { get; set; }

        private string GetHtmlUrl(string[] topics)
        {
            var rnd = new Random();

            int topic = rnd.Next(0, topics.Length - 1);
            var searchTerm = topics[topic].Trim();

            var url = "https://www.google.com/search?q=" + searchTerm + "&tbm=isch";
            var data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using(Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                {
                    return "";
                }
                using (var sr = new StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }

        private List<string> GetUrls(string html)
        {
            var ndx = html.IndexOf("class=\"images_table\"", StringComparison.Ordinal);
            ndx = html.IndexOf("<img", ndx, StringComparison.Ordinal);
            var urlList = new List<string>();
            while(ndx >= 0)
            {
                ndx = html.IndexOf("src=\"", ndx, StringComparison.Ordinal);
                ndx = ndx + 5;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                urlList.Add(url);
                ndx = html.IndexOf("<img", ndx, StringComparison.Ordinal);
            }
            _urls = urlList;
            return _urls;
        }

        public GoogleImageGetter(string[] topics)
        {
            var html = GetHtmlUrl(topics);
            Urls = GetUrls(html);
        }
    }
}
