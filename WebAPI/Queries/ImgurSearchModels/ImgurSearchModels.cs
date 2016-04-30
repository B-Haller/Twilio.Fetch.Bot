using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioFetchBot.ImgurSearchModels
{
    public class ImgurSubredditSearchCriteria
    {
        public string Subreddit { get; set; }
        public string Sort { get; set; }
        public int Page { get; set; }
        public string Window { get; set; }

        public ImgurSubredditSearchCriteria(string subreddit, string sort = null, string window = null, int? page = null)
        {
            Subreddit = subreddit;
            Sort = sort;
            Window = window;
            Page = page ?? default(int);
        }
    }
    public class ImgurMemeSearchCriteria
    {

    }

}
