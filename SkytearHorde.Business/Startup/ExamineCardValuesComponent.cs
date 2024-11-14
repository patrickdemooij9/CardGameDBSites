using Examine;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Startup
{
    public class ExamineCardValuesComponent : IComponent
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardPriceService _cardPriceService;
        private readonly CardService _cardService;

        public ExamineCardValuesComponent(IExamineManager examineManager, IUmbracoContextFactory umbracoContextFactory,
            CardPriceService cardPriceService, CardService cardService)
        {
            _examineManager = examineManager;
            _umbracoContextFactory = umbracoContextFactory;
            _cardPriceService = cardPriceService;
            _cardService = cardService;
        }

        public void Initialize()
        {
            if (!_examineManager.TryGetIndex("ExternalIndex", out var index))
            {
                return;
            }

            index.TransformingIndexValues += Index_TransformingIndexValues;
        }

        private void Index_TransformingIndexValues(object? sender, IndexingItemEventArgs e)
        {
            if (!int.TryParse(e.ValueSet.Id, out var contentId)) return;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            var publishedContent = ctx.UmbracoContext.Content?.GetById(contentId);
            if (publishedContent is null) return;

            var updatedValues = e.ValueSet.Values.ToDictionary(x => x.Key, x => x.Value.ToList());
            if (publishedContent is Card)
            {
                var card = _cardService.Get(contentId);
                if (card is null) return;

                updatedValues = GetValues(updatedValues, card, ctx.UmbracoContext);
            }
            else if (publishedContent is CardVariant cardVariant)
            {
                var variant = _cardService.GetVariants(publishedContent.Parent!.Id).FirstOrDefault(it => it.VariantId == cardVariant.Id);
                if (variant is null) return;

                updatedValues = GetValues(updatedValues, variant, ctx.UmbracoContext);
            }

            e.SetValues(updatedValues.ToDictionary(x => x.Key, x => (IEnumerable<object>)x.Value));
        }

        public void Terminate()
        {

        }

        private Dictionary<string, List<object>> GetValues(Dictionary<string, List<object>> updatedValues, Entities.Models.Business.Card card, IUmbracoContext ctx)
        {
            foreach (var ability in card.Attributes)
            {
                if (ability.Value is HeaderTextItemAbilityValue) continue; // No idea how to really parse this yet...

                var values = ability.Value.GetValues().Select(it => it.Replace(" ", ""));
                updatedValues[$"CustomField.{ability.Key!.Name}"] = values.ToList<object>();
            }

            var setName = card.SetName.Replace(" ", "");
            updatedValues["CustomField.Set Name"] = [setName];
            updatedValues["CustomField.SetId"] = [card.SetId];
            updatedValues["Name"] = [card.DisplayName];
            updatedValues["VariantType"] = [card.VariantTypeId ?? 0];

            var set = ctx.Content?.GetById(card.SetId) as Set;
            if (set != null)
            {
                var sortOrder = (set.Cards?.Select(it => it.Id).ToArray() ?? []).IndexOf(card.VariantId);
                if (sortOrder >= 0)
                {
                    updatedValues["sortOrder"] = [sortOrder];
                }
            }

            updatedValues["DecksOnly"] = [card.HideFromDecks ? 0 : 1];

            var prices = _cardPriceService.GetPrices(card.BaseId);
            var price = prices.FirstOrDefault()?.Prices.FirstOrDefault(it => it.VariantId == card.VariantId);
            if (price != null)
            {
                updatedValues["Price"] = [(int)(price.MainPrice * 100)];
            }
            else
            {
                updatedValues["Price"] = [0];
            }
            return updatedValues;
        }
    }
}
