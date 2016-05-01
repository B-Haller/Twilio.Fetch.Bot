using TwilioTestApp.WebAPI.Tokens;
using System.Configuration;

namespace TwilioFetchBot.Tokens
{
    public class ImgurCredentials : ITokens
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }

        public ImgurCredentials()
        {
            AccountSid = ConfigurationManager.AppSettings["ImgurClientId"];
            AuthToken = ConfigurationManager.AppSettings["ImgurClientSecret"];
        }
    }
}
