using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Facts
{
    public abstract class FactGeneratorBase : IFactGenerator
    {
        public abstract string Key { get; }
        public virtual bool IsAutomatic => true;
        public abstract GameFact? Generate(FactContext context, IReadOnlyDictionary<string, string>? parameters);

        protected static int GetInt(Card card, string key)
        {
            var value = card.GetMultipleCardAttributeValue(key)?.FirstOrDefault();
            return value != null && int.TryParse(value, out var number) ? number : 0;
        }

        protected static bool HasInt(Card card, string key)
            => int.TryParse(card.GetMultipleCardAttributeValue(key)?.FirstOrDefault(), out _);

        protected static string[] GetTraits(Card card)
            => card.GetMultipleCardAttributeValue("Traits") ?? [];

        protected static bool IsUnit(Card card)
            => card.GetMultipleCardAttributeValue("Card Type")?.Contains("Unit") is true;

        protected static string? ImageRel(Card card)
            => card.Image?.Url(mode: UrlMode.Relative);

        /// <summary>
        /// The scope the fact covers, phrased for a hook sentence: "the game" when unscoped,
        /// or e.g. "the LOF set" when the facts are limited to a single set. Lets a hook read
        /// naturally in both cases ("… the most traits in {ScopeLabel}?").
        /// </summary>
        protected static string ScopeLabel(FactContext context)
        {
            var set = context.Set;
            if (set is null) return "the game";
            var code = string.IsNullOrWhiteSpace(set.SetCode) ? set.DisplayName : set.SetCode.ToUpperInvariant();
            return $"the {code} set";
        }

        protected static string Money(double value)
            => "$" + value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    }
}
