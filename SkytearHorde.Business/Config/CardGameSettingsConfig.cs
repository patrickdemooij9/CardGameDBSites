namespace SkytearHorde.Business.Config
{
    public class CardGameSettingsConfig
    {
        public string SkillSetApiKey { get; set; }
        public string TcgPlayerApiKey { get; set; }
        public string TcgPlayerApiSecret { get; set; }
        public string SentryLink { get; set; }
        public string RecaptchaSecret { get; set; }
        public string CardReaderApiKey { get; set; }
        public string[] AdminMembers { get; set; } = [];
        public MeleeGgConfig MeleeGg { get; set; } = new();
    }

    public class MeleeGgConfig
    {
        /// <summary>Whether the melee.gg tournament sync is enabled.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>The melee.gg game slug to filter tournaments (e.g. "star-wars-unlimited").</summary>
        public string GameSlug { get; set; } = "star-wars-unlimited";

        /// <summary>The internal SiteId to assign imported tournaments to.</summary>
        public int SiteId { get; set; } = 1;

        /// <summary>The internal FormatId (SquadSettings TypeID) to use for imported tournaments.</summary>
        public int FormatId { get; set; } = 1;

        /// <summary>How many days back to look for new tournaments on each sync run.</summary>
        public int LookbackDays { get; set; } = 30;

        /// <summary>melee.gg API base URL.</summary>
        public string BaseUrl { get; set; } = "https://melee.gg";
    }
}
