namespace TwilioTestApp.WebAPI.Tokens
{
    interface ITokens
    {
        string AccountSid { get; set; }
        string AuthToken { get; set; }
    }
}
