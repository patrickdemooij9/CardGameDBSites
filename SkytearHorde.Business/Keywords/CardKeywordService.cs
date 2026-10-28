using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using CardAttribute = SkytearHorde.Entities.Generated.CardAttribute;

namespace SkytearHorde.Business.Keywords
{
    public class CardKeywordService
    {
        private const string MultipleSuffix = "_multiple";
        private static readonly Guid MultiTextAbilityKey = new("df117396-beb9-47d5-9323-4469ac12326a");

        private readonly CardImportQueueService _cardImportQueueService;
        private readonly ISiteService _siteService;
        private readonly CardService _cardService;
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly BlockListJsonService _blockListJsonService;

        public CardKeywordService(
            CardImportQueueService cardImportQueueService,
            ISiteService siteService,
            CardService cardService,
            IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            BlockListJsonService blockListJsonService)
        {
            _cardImportQueueService = cardImportQueueService;
            _siteService = siteService;
            _cardService = cardService;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _blockListJsonService = blockListJsonService;
        }

        public void AddDetectedKeywords(ImportModel model)
        {
            var config = _cardImportQueueService.GetKeywordDetectionConfig();
            if (config is null) return;

            var texts = config.AbilityAttributes
                .Select(it => model.Properties.GetValueOrDefault(it))
                .OfType<string>()
                .ToArray();
            if (texts.Length == 0) return;

            var detected = new KeywordDetector(config).Detect(model.Name, texts);
            foreach (var (relation, attribute) in GetRelationAttributes())
            {
                model.Properties.Remove(attribute.Name);
                model.Properties.Remove(attribute.Name + MultipleSuffix);

                var values = ValuesFor(detected, relation);
                if (values.Length > 0)
                    model.Properties[attribute.Name + MultipleSuffix] = string.Join(",", values);
            }
        }

        public KeywordVocabulary GetUsedVocabulary()
        {
            var config = _cardImportQueueService.GetKeywordDetectionConfig();
            if (config is null) return new KeywordVocabulary([], []);

            var used = GetRelationAttributes().Values
                .SelectMany(it => _cardService.GetCardValues(it.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new KeywordVocabulary(
                [.. config.TriggeredAbilities.Where(used.Contains)],
                [.. config.Keywords.Where(used.Contains)]);
        }

        public KeywordBackfillResult Backfill(bool dryRun)
        {
            var config = _cardImportQueueService.GetKeywordDetectionConfig()
                ?? throw new InvalidOperationException("The game config has no keywordDetection section.");
            var detector = new KeywordDetector(config);
            var udiByRelation = GetRelationAttributes()
                .ToDictionary(it => it.Key, it => Udi.Create(Constants.UdiEntityType.Document, it.Value.Key).ToString());
            var keywordUdis = udiByRelation.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var cards = _cardService.GetAll().ToArray();
            var changed = new List<KeywordBackfillCard>();

            foreach (var card in cards)
            {
                var texts = config.AbilityAttributes.SelectMany(it => card.GetMultipleCardAttributeValue(it) ?? []).ToArray();
                var detected = detector.Detect(card.DisplayName, texts);
                var desired = udiByRelation
                    .Select(it => (Udi: it.Value, Values: ValuesFor(detected, it.Key)))
                    .Where(it => it.Values.Length > 0)
                    .ToArray();

                var content = _contentService.GetById(card.BaseId);
                if (content is null) continue;

                var items = _blockListJsonService.Deserialize(content.GetValue<string>("attributes"));
                var current = items
                    .Where(it => keywordUdis.Contains(it.GetValueOrDefault("ability") ?? ""))
                    .ToDictionary(it => it["ability"], it => SplitValueList(it.GetValueOrDefault("valueList")), StringComparer.OrdinalIgnoreCase);

                var isUnchanged = current.Count == desired.Length
                    && desired.All(it => current.TryGetValue(it.Udi, out var values) && values.SequenceEqual(it.Values));
                if (isUnchanged) continue;

                changed.Add(new KeywordBackfillCard(card.BaseId, card.DisplayName, desired.ToDictionary(
                    it => udiByRelation.First(relation => relation.Value == it.Udi).Key.ToString(),
                    it => it.Values)));
                if (dryRun) continue;

                var updated = items
                    .Where(it => !keywordUdis.Contains(it.GetValueOrDefault("ability") ?? ""))
                    .Concat(desired.Select(it => new Dictionary<string, string>
                    {
                        ["ability"] = it.Udi,
                        [BlockListJsonService.ContentTypeKeyAlias] = MultiTextAbilityKey.ToString(),
                        ["valueList"] = string.Join(Environment.NewLine, it.Values)
                    }))
                    .ToList();

                content.SetValue("attributes", _blockListJsonService.Serialize(updated, MultiTextAbilityKey));
                _contentService.SaveAndPublish(content, []);
            }

            return new KeywordBackfillResult(cards.Length, dryRun, changed);
        }

        public SortedDictionary<KeywordRelation, CardAttribute> GetRelationAttributes()
        {
            var result = new SortedDictionary<KeywordRelation, CardAttribute>();
            foreach (var attribute in _siteService.GetAllAttributes())
            {
                if (Enum.TryParse<KeywordRelation>(attribute.KeywordRelation, out var relation))
                    result.TryAdd(relation, attribute);
            }
            return result;
        }

        private static string[] ValuesFor(IEnumerable<DetectedKeyword> detected, KeywordRelation relation)
        {
            return detected.Where(it => it.Relation == relation).Select(it => it.Keyword).Distinct().Order().ToArray();
        }

        // The importer stores newline-separated text, the backoffice stores a JSON array.
        private static string[] SplitValueList(string? valueList)
        {
            if (string.IsNullOrWhiteSpace(valueList)) return [];

            var values = valueList.TrimStart().StartsWith('[')
                ? JsonSerializer.Deserialize<string[]>(valueList) ?? []
                : valueList.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return values.Order().ToArray();
        }
    }
}
