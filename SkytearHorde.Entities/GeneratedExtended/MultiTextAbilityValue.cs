using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class MultiTextAbilityValue : IAbilityValue
    {
        public string GetAbilityValue()
        {
            return string.Join(",", Values ?? Array.Empty<string>());
        }

        public string[] GetValues()
        {
            return Values?.ToArray() ?? Array.Empty<string>();
        }

        public IEnumerable<DiscordField> GetDiscordField()
        {
            var ability = (Ability as CardAttribute);
            return new[] {
                new DiscordField
                {
                    Name = ability.DisplayName,
                    Value = GetAbilityValue(),
                    Inline = ability.IsDiscordInline
                }
            };
        }
    }
}
