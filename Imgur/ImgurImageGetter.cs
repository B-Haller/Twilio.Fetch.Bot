using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TwilioFetchBot.Tokens;
using TwilioFetchBot.ImgurResponseModels;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TwilioFetchBot.Google
{
    public class ImgurImageGetter
    {
        //what user is searching for
        public string SearchCriteria { get; set; }
        //url created based on the search type and query type
        public string ImgurSeachUrl { get; set; }
        //list returned from image searches
        public List<GalleryImages> GalleryImagesList { get; set; }
        //list returned from filtered results
        public List<GalleryImages> FilteredGalleryImagesList { get; set; }
        public string WebRequestError { get; set; }
        //internal counters
        private int _pages { get; set; }
        private int _searches { get; set; }

        /// <summary>
        /// Search type available
        /// </summary>
        public enum SearchType
        {
            Subreddit = 1,
            Meme,
            Gallery,
            Unknown
        }
        /// <summary>
        /// Enums for query types available when searching galleries
        /// </summary>
        public enum QueryType
        {
            q_all = 1,
            q_any,
            q_exactly,
            q_not,
            q_type,
            q_size_px,
            Unknown
        }

        /// <summary>
        /// Response model: GalleryImages
        /// Query model: ImgurSubredditSearchCritera
        /// </summary>
        /// <param name="subreddit">Required: pics - A valid subreddit name</param>
        /// <param name="sort">Optional: time | top - defaults to time</param>
        /// <param name="window">Optional: integer - the data paging number</param>
        /// <param name="page">Optional: Change the date range of the request if the sort is "top", day | week | month | year | all, defaults to week</param>
        /// <returns>List of Gallery Images</returns>
        public List<GalleryImages> GetImageBySubreddit(string subreddit, string sort = null, string window = null, int? page = null)
        {
            var text = String.Empty;
            SearchCriteria = subreddit;
            ImgurSeachUrl = "https://api.imgur.com/3/gallery/r/" + $"{SearchCriteria}/{sort}/{window}/{page}";
            var response = ImgurRestRequest(ImgurSeachUrl);

            text = GetStream(response);
            var galleryImageRoot = JToken.Parse(text).ToObject<GalleryImageRoot>();
            
            GalleryImagesList = galleryImageRoot.data;

            return GalleryImagesList;
        }
        /// <summary>
        ///  Response model: GalleryImages
        ///  Query model: ImgurMemeSearchCriteria
        /// </summary>
        /// <param name="sort">Optional: viral | time | top</param>
        /// <param name="window">Optional: Change the date range of the request if the sort is "top", day | week | month | year | all, defaults to week</param>
        /// <param name="page">Optional: integer - the data paging number</param>
        /// <returns>List of Gallery Images</returns>
        public List<GalleryImages> GetImageByMeme(string sort = null, string window = null, int? page = null)
        {
            var text = String.Empty;
            ImgurSeachUrl = "https://api.imgur.com/3/g/memes/" + $"{sort}/{window}/{page}";
            var response = ImgurRestRequest(ImgurSeachUrl);

            text = GetStream(response);
            var GalleryImageRoot = JToken.Parse(text).ToObject<GalleryImageRoot>();

            GalleryImagesList = GalleryImageRoot.data;

            return GalleryImagesList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qSearch">Search Query</param>
        /// <param name="qType">Specific type of search</param>
        /// <param name = "sort" >Optional: viral | time | top</param>
        /// <param name="window">Optional: Change the date range of the request if the sort is "top", day | week | month | year | all, defaults to week</param>
        /// <param name="page">Optional: integer - the data paging number</param>
        /// <returns>List of Gallery Images</returns>
        public List<GalleryImages> GetImageByGallery(string qSearch, QueryType qType = QueryType.Unknown, string sort = null, string window = null, int? page = null)
        {
            var text = String.Empty;
            SearchCriteria = qSearch;
            ImgurSeachUrl = "https://api.imgur.com/3/gallery/search?" + $"{BuildQueryType(qType)}{qSearch}/{sort}/{window}/{page}";
            var response = ImgurRestRequest(ImgurSeachUrl);

            text = GetStream(response);
            var galleryImageRoot = JToken.Parse(text).ToObject<GalleryImageRoot>();

            GalleryImagesList = galleryImageRoot.data;

            return GalleryImagesList;
        }
        /// <summary>
        /// Builds custom query string for specific gallery search types
        /// </summary>
        /// <param name="qType">Quesy type enum</param>
        /// <returns>String to be used for determining query type</returns>
        private string BuildQueryType(QueryType qType)
        {
            switch (qType)
            {
                case QueryType.q_all:
                    return "q_all=";
                case QueryType.q_any:
                    return "q_any=";
                case QueryType.q_exactly:
                    return "q_exactly=";
                case QueryType.q_not:
                    return "q_not=";
                case QueryType.q_type:
                    return "q_type=";
                case QueryType.q_size_px:
                    return "q_size_px=";
                case QueryType.Unknown:
                    return "q_all=";
                default:
                    return "q_all=";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        private List<GalleryImages> GetCorrectGalleryListBySearchType(SearchType searchType, string searchQuery)
        {
            switch (searchType)
            {
                case SearchType.Subreddit:
                    return GetImageBySubreddit(searchQuery);
                case SearchType.Meme:
                    return GetImageByMeme();
                case SearchType.Gallery:
                    return GetImageByGallery(searchQuery);
                case SearchType.Unknown:
                    return GetImageByMeme();
                default:
                    return GetImageByMeme();
            }
        }

        private static string GetStream(WebResponse response)
        {
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            return text;
        }

        private WebResponse ImgurRestRequest(string url)
        {

            var imgurCredentials = new ImgurCredentials();
            HttpWebRequest wrGetURL = (HttpWebRequest)WebRequest.Create(url);
            wrGetURL.Method = "GET";
            wrGetURL.ContentType = "application/json; charset=utf-8";
            wrGetURL.Headers.Add($"Authorization: Client-ID {imgurCredentials.AccountSid}");

            var response = wrGetURL.GetResponse();

            return response;

        }


        public List<GalleryImages> GetImageByTitleSearch(List<GalleryImages> listToSearch, SearchType searchType, string searchQuery)
        {
            List<GalleryImages> results = TitleSearchQuery(listToSearch, searchQuery);

            while (!results.Any() && _searches < 100)
            {
                _pages += 1;
                _searches += 1;
                listToSearch = GetCorrectGalleryListBySearchType(searchType, searchQuery);
                results = GetImageByTitleSearch(listToSearch, searchType, searchQuery);
            }

            return results;
        }

        public static List<GalleryImages> TitleSearchQuery(List<GalleryImages> listToSearch, string searchQuery)
        {
            List<GalleryImages> results = new List<GalleryImages>();

            foreach (var result in listToSearch)
            {
                if (Regex.IsMatch(result.title, @"\b" + searchQuery + @"\b", RegexOptions.IgnoreCase))
                {
                    results.Add(result);
                }
                else
                {
                    continue;
                }
            }
            return results;
        }

        public ImgurImageGetter(SearchType searchType, string searchCriteria)
        {
            _pages = 1;
            _searches = 1;
            SearchCriteria = searchCriteria;
            GalleryImagesList = GetCorrectGalleryListBySearchType(searchType, SearchCriteria);
        }
    }

}
