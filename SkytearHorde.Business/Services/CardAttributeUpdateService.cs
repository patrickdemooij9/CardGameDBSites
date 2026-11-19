using SkytearHorde.Business.Services.Site;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.Services
{
    public class CardAttributeUpdateService
    {
        // Re-serializing a card whose blocks include a header ability would flatten its nested block list.
        private static readonly string HeaderAbilityTypeKey = "ae3e7551-1f43-4784-aec1-6771b7ddd018";

        private readonly CardService _cardService;
        private readonly ISiteService _siteService;
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly BlockListJsonService _blockListJsonService;

        public CardAttributeUpdateService(
            CardService cardService,
            ISiteService siteService,
            IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            BlockListJsonService blockListJsonService)
        {
            _cardService = cardService;
            _siteService = siteService;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _blockListJsonService = blockListJsonService;
        }

        /// <summary>
        /// Sets a text attribute on every card of the current site whose Type is one of <paramref name="cardTypes"/>.
        /// Only cards that already have the attribute are changed.
        /// </summary>
        public CardAttributeUpdateResult SetTextAttribute(string attributeName, string value, IReadOnlyCollection<string> cardTypes, bool dryRun)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var result = new CardAttributeUpdateResult { DryRun = dryRun };

            var attribute = _siteService.GetAllAttributes().FirstOrDefault(it => it.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Attribute '{attributeName}' does not exist on this site.");
            var attributeUdi = Udi.Create(Constants.UdiEntityType.Document, attribute.Key).ToString();

            var cards = _cardService.GetAll(includeVariants: true)
                .Where(card => card.GetMultipleCardAttributeValue("Type")?.Any(type => cardTypes.Contains(type, StringComparer.OrdinalIgnoreCase)) == true);

            var contentIds = cards.SelectMany(card => new[] { card.BaseId, card.VariantId }).Distinct();
            foreach (var content in _contentService.GetByIds(contentIds))
            {
                var blocks = _blockListJsonService.Deserialize(content.GetValue<string>("attributes"));
                var target = blocks.FirstOrDefault(block => block.TryGetValue("ability", out var udi) && udi.Equals(attributeUdi, StringComparison.OrdinalIgnoreCase));
                if (target is null) continue;

                if (!target.TryGetValue("value", out var current))
                {
                    result.Skipped.Add($"{content.Name} ({attributeName} is not a single text value)");
                    continue;
                }

                if (current == value)
                {
                    result.AlreadyCorrect++;
                    continue;
                }

                if (blocks.Any(block => block.TryGetValue(BlockListJsonService.ContentTypeKeyAlias, out var typeKey) && typeKey.Equals(HeaderAbilityTypeKey, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Skipped.Add($"{content.Name} (has a header ability, update by hand)");
                    continue;
                }

                result.Updated.Add($"{content.Name}: {current} -> {value}");
                if (dryRun) continue;

                target["value"] = value;
                content.SetValue("attributes", _blockListJsonService.Serialize(blocks, Guid.Parse(target[BlockListJsonService.ContentTypeKeyAlias])));
                _contentService.SaveAndPublish(content, []);
            }

            if (!dryRun && result.Updated.Count > 0)
                _cardService.ClearCache();

            return result;
        }
    }
}
