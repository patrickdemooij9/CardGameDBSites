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
    }
}
