namespace SkytearHorde.Entities.Models.Business
{
    public class DiscordField
    {
        public required string Name { get; set; }
        public required string Value { get; set; }
        public bool Inline { get; set; } = false;
    }
}
