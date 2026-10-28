namespace SkytearHorde.Business.Keywords
{
    public class KeywordDetectionConfig
    {
        public List<string> AbilityAttributes { get; set; } = [];
        public List<string> Keywords { get; set; } = [];
        public List<string> TriggeredAbilities { get; set; } = [];
        public List<string> IgnoredPhrases { get; set; } = [];
    }
}
