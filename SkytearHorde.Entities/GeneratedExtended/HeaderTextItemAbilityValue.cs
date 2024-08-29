using SkytearHorde.Entities.Models.Business;
using System.Text;
using Umbraco.Cms.Core.Models.Blocks;

namespace SkytearHorde.Entities.Generated
{
    public partial class HeaderTextItemAbilityValue : IAbilityValue
    {
        public string GetAbilityValue()
        {
            var stringBuilder = new StringBuilder();
            foreach(var item in Items?.OfType<BlockListItem<HeaderTextItem>>() ?? Enumerable.Empty<BlockListItem<HeaderTextItem>>())
            {
                stringBuilder.AppendLine($"[Header]{item.Content.Header}[Header][Text]{item.Content.Text}[Text];");
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<DiscordField> GetDiscordField()
        {
            foreach (var item in Items?.OfType<BlockListItem<HeaderTextItem>>() ?? Enumerable.Empty<BlockListItem<HeaderTextItem>>())
            {
                yield return new DiscordField
                {
                    Name = item.Content.Header,
                    Value = item.Content.Text
                };
            }
        }

        public string[] GetValues()
        {
            return Array.Empty<string>();
        }
    }
}
