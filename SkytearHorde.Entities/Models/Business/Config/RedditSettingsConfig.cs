namespace SkytearHorde.Entities.Models.Business.Config
{
    public class RedditSettingsConfig
    {
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Subreddit { get; set; }
    }
}
