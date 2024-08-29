using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class TextAbilityValue : IAbilityValue
    {
        public string GetAbilityValue()
        {
            return Value!;
        }

        public string[] GetValues()
        {
            return new[] { Value! };
        }

        public IEnumerable<DiscordField> GetDiscordField()
        {
            var ability = (Ability as CardAttribute);
            return new[] {
                new DiscordField
                {
                    Name = ability.DisplayName,
                    Value = Value,
                    Inline = ability.IsDiscordInline
                }
            };
        }
    }
}
