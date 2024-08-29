namespace SkytearHorde.Entities.Models.Business.Config
{
    public class KeywordImageConfig
    {
        public string Keyword { get; }
        public string Image { get; }
        public string? DiscordIcon { get; set; }

        public KeywordImageConfig(string keyword, string image)
        {
            Keyword = keyword;
            Image = image;
        }
    }
}
