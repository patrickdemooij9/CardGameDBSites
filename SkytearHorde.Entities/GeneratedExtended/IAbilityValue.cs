using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial interface IAbilityValue
    {
        string GetAbilityValue();
        string[] GetValues();
        IEnumerable<DiscordField> GetDiscordField();
        CardAttribute GetAbility()
        {
            return (CardAttribute) Ability!;
        }
    }
}
