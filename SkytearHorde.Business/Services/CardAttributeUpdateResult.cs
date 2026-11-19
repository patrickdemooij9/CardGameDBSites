namespace SkytearHorde.Business.Services
{
    public class CardAttributeUpdateResult
    {
        public bool DryRun { get; set; }
        public List<string> Updated { get; set; } = [];
        public int AlreadyCorrect { get; set; }
        public List<string> Skipped { get; set; } = [];
    }
}
