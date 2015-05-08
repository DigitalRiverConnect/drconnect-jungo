namespace Jungo.Models.Oauth20
{
    // names are OAuth 2.0 standard; sorry Resharper, you lose!
    public class Token
    {
        // ReSharper disable once InconsistentNaming
        public string access_token { get; set; }
        // ReSharper disable once InconsistentNaming
        public string token_type { get; set; }
        // ReSharper disable once InconsistentNaming
        public int expires_in { get; set; } // seconds
        // ReSharper disable once InconsistentNaming
        public string refresh_token { get; set; }
    }
}
