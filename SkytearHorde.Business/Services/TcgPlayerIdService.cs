using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class TcgPlayerIdService
    {
        private const string AttributesAlias = "attributes";
        private static readonly Guid TextAbilityContentTypeKey = new("A4AC0B27-5103-4E6C-A6E5-111BA1500F26");

        private readonly ISiteService _siteService;
        private readonly IContentService _contentService;
        private readonly BlockListJsonService _blockListJsonService;

        public TcgPlayerIdService(ISiteService siteService, IContentService contentService, BlockListJsonService blockListJsonService)
        {
            _siteService = siteService;
            _contentService = contentService;
            _blockListJsonService = blockListJsonService;
        }

        public bool TrySetTcgPlayerId(int variantId, string? tcgPlayerId)
        {
            var content = _contentService.GetById(variantId);
            var attributeKey = _siteService.GetRoot()
                .FirstChild<Data>()?
                .FirstChild<CardAttributeContainer>()?
                .FirstChild<CardAttribute>(it => it.Name.Equals(CardPriceSyncService.TcgPlayerIdAttribute))?
                .Key;
            if (content is null || attributeKey is null) return false;

            var abilityUdi = Udi.Create(Constants.UdiEntityType.Document, attributeKey.Value).ToString();
            var attributes = _blockListJsonService.Deserialize(content.GetValue<string>(AttributesAlias));
            attributes.RemoveAll(it => it.TryGetValue("ability", out var ability)
                && UdiParser.TryParse(ability, out var udi)
                && (udi as GuidUdi)?.Guid == attributeKey);

            if (!string.IsNullOrWhiteSpace(tcgPlayerId))
            {
                attributes.Add(new Dictionary<string, string>
                {
                    { "ability", abilityUdi },
                    { "value", tcgPlayerId.Trim() }
                });
            }

            content.SetValue(AttributesAlias, _blockListJsonService.Serialize(attributes, TextAbilityContentTypeKey));
            _contentService.SaveAndPublish(content, []);
            return true;
        }
    }
}
