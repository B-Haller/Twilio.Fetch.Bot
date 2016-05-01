using System.Configuration;
using TwilioTestApp.WebAPI.Tokens;

namespace TwilioFetchBot.Tokens
{
    public class TwilioCredentials : ITokens
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }

        public TwilioCredentials()
        {
            AccountSid = ConfigurationManager.AppSettings["TwilioClientId"];
            AuthToken = ConfigurationManager.AppSettings["TwilioClientSecret"];
        }

    }
}
