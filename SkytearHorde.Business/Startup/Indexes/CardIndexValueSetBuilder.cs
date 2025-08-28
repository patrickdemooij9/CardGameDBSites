using Examine;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Startup.Indexes
{
    internal class CardIndexValueSetBuilder : IValueSetBuilder<Card>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardPriceService _cardPriceService;

        public CardIndexValueSetBuilder(IUmbracoContextFactory umbracoContextFactory, CardPriceService cardPriceService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _cardPriceService = cardPriceService;
        }

        public IEnumerable<ValueSet> GetValueSets(params Card[] contents)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            foreach (var content in contents)
            {
                var indexValues = new Dictionary<string, IEnumerable<object>>
                {
                    // this is a special field used to display the content name in the Examine dashboard
                    [UmbracoExamineFieldNames.NodeNameFieldName] = [content.DisplayName!],
                    ["name"] = [content.DisplayName!],
                    ["id"] = [content.VariantId],
                };

                HandleCardValues(indexValues, content, ctx.UmbracoContext);
                HandleSiteId(indexValues, content, ctx.UmbracoContext);

                yield return new ValueSet(content.VariantId.ToString(), IndexTypes.Content, indexValues);
            }
        }

        private void HandleCardValues(Dictionary<string, IEnumerable<object>> updatedValues, Card card, IUmbracoContext ctx)
        {
            foreach (var ability in card.Attributes)
            {
                if (ability.Value is HeaderTextItemAbilityValue) continue; // No idea how to really parse this yet...

                var values = ability.Value.GetValues().Select(it => it.Replace(" ", "")).ToArray();
                updatedValues[$"CustomField.{ability.Key}"] = values.ToList<object>();
                if (ability.Value.GetAbility().IsMultiValue)
                {
                    updatedValues[$"CustomField.{ability.Key}.Amount"] = [values.Length];
                }
                foreach (var value in ability.Value.GetAbility().CountValues ?? [])
                {
                    updatedValues[$"CustomField.{ability.Key}.{value}.Amount"] = [values.Count(it => it == value)];
                }
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
        }

        private void HandleSiteId(Dictionary<string, IEnumerable<object>> updatedValues, Card card, IUmbracoContext ctx)
        {
            var content = ctx.Content!.GetById(card.BaseId);
            if (content is null) return;

            var siteId = content.Root().FirstChild<Settings>()?.FirstChild<SiteSettings>()?.SiteId;
            if (siteId is null) return;

            updatedValues["siteId"] = [siteId];
        }
    }
}
