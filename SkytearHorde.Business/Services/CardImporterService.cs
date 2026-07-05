using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    /// <summary>
    /// Shared card-creation logic used by both the manual importer (ImporterController)
    /// and the card import queue approval flow. Mirrors the original ImporterController.ImportModels.
    /// </summary>
    public class CardImporterService
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly MediaFileManager _mediaFileManager;
        private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ISiteService _siteService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

        public CardImporterService(
            IUmbracoContextFactory umbracoContextFactory,
            IContentService contentService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            ISiteService siteService,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
            _mediaService = mediaService;
            _mediaFileManager = mediaFileManager;
            _mediaUrlGenerators = mediaUrlGenerators;
            _shortStringHelper = shortStringHelper;
            _siteService = siteService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        }

        /// <summary>
        /// Creates a new card image media item from raw bytes and returns its id.
        /// </summary>
        public int CreateImageMediaFromBytes(string name, byte[] bytes)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var cardImageParentId = _siteService.GetSettings().FirstChild<SiteSettings>()?.CardImageRoot?.Id
                ?? throw new InvalidOperationException("Card image root not found.");

            var media = _mediaService.CreateMediaWithIdentity(name, cardImageParentId, Image.ModelTypeAlias);
            using var stream = new MemoryStream(bytes);
            media.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, "umbracoFile", name + ".png", stream);
            _mediaService.Save(media);
            return media.Id;
        }

        /// <summary>
        /// Creates or updates card content nodes for each model, linking them to their set
        /// and writing their attributes. Moved verbatim from ImporterController.ImportModels.
        /// </summary>
        public void Import(IEnumerable<ImportModel> models)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var parentContainer = _siteService.GetRoot().FirstChild<Data>();
            var dataContainer = parentContainer?.FirstChild<SetContainer>();
            var cardsContainer = parentContainer?.FirstChild<CardContainer>();
            var attributeContainer = parentContainer?.FirstChild<CardAttributeContainer>();
            if (dataContainer is null)
                throw new Exception("No data container found");

            var variants = parentContainer?.FirstChild<VariantsContainer>()?.Children<Variant>()?.ToArray() ?? Array.Empty<Variant>();

            var abilityCache = new Dictionary<string, Guid>();
            foreach (var model in models)
            {
                var set = dataContainer.FirstChild<Set>(it => it.Name.Equals(model.SetName));
                var setId = set?.Id;
                if (set is null)
                {
                    var newSet = _contentService.Create(model.SetName, dataContainer.Id, Set.ModelTypeAlias);
                    var result = _contentService.SaveAndPublish(newSet).Content;
                    setId = result.Id;
                    set = dataContainer.FirstChild<Set>(it => it.Name.Equals(model.SetName))!;
                }

                if (model.Id == 0 || model.Id is null)
                {
                    //Create new
                    var newCard = _contentService.Create(model.Name, model.ParentId ?? cardsContainer?.Id ?? set.Id, model.ParentId.HasValue ? CardVariant.ModelTypeAlias : Card.ModelTypeAlias);
                    model.Id = _contentService.SaveAndPublish(newCard).Content.Id;
                }

                var searchId = model.Id;
                if (searchId == 0 || searchId is null)
                {
                    throw new Exception("Card did not get created!");
                }

                var card = _contentService.GetById(searchId.Value);
                if (card is null)
                {
                    throw new Exception($"No card found with ID: {searchId}");
                }

                if (model.ParentId is null && cardsContainer != null && card.ParentId != cardsContainer.Id)
                {
                    _contentService.Move(card, cardsContainer.Id);
                }

                var attributes = new List<Dictionary<string, string>>();
                foreach (var cardAttribute in model.Properties)
                {
                    var attributeKey = cardAttribute.Key;
                    var isMultipleText = attributeKey.EndsWith("_multiple");
                    if (isMultipleText)
                    {
                        attributeKey = attributeKey.TrimEnd("_multiple");
                    }

                    var existingAttributeGuid = abilityCache.ContainsKey(attributeKey) ? abilityCache[attributeKey] : attributeContainer.FirstChild<CardAttribute>(it => it.Name.Equals(attributeKey))?.Key;

                    if (existingAttributeGuid is null)
                    {
                        var newAttribute = _contentService.Create(attributeKey, attributeContainer.Id, CardAttribute.ModelTypeAlias);
                        if (isMultipleText)
                        {
                            newAttribute.SetValue("isMultiValue", true);
                        }
                        existingAttributeGuid = _contentService.SaveAndPublish(newAttribute).Content.Key;
                        abilityCache.Add(attributeKey, existingAttributeGuid.Value);
                    }

                    var isHeaderValue = cardAttribute.Value.Contains("[Header]");
                    var contentTypeKey = isHeaderValue ? new Guid("ae3e7551-1f43-4784-aec1-6771b7ddd018") : isMultipleText ? new Guid("df117396-beb9-47d5-9323-4469ac12326a") : new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26");

                    var values = new Dictionary<string, string>()
                        {
                            {"ability", Udi.Create(Constants.UdiEntityType.Document, existingAttributeGuid.Value).ToString() },
                            {"contentTypeKey", contentTypeKey.ToString()}
                        };

                    if (isHeaderValue)
                    {
                        var headerValueItems = new List<Dictionary<string, string>>();
                        foreach (var item in cardAttribute.Value.Split(';'))
                        {
                            if (string.IsNullOrWhiteSpace(item)) continue;

                            var firstHeaderIndex = item.IndexOf("[Header]") + "[Header]".Length;
                            var firstTextIndex = item.IndexOf("[Text]") + "[Text]".Length;

                            var header = item.Substring(firstHeaderIndex, item.LastIndexOf("[Header]") - firstHeaderIndex);
                            var text = item.Substring(firstTextIndex, item.LastIndexOf("[Text]") - firstTextIndex);

                            headerValueItems.Add(new Dictionary<string, string>
                                {
                                    { "header", header },
                                    { "text", text }
                                });
                        }
                        values.Add("items", BlockListCreatorHelper.GetBlockListJsonFor(headerValueItems, new Guid("89468ec9-2fea-426e-ad61-304a1b5f0ece")));
                    }
                    else if (isMultipleText)
                    {
                        values.Add("values", cardAttribute.Value.Replace(",", Environment.NewLine));
                    }
                    else
                    {
                        values.Add("value", cardAttribute.Value);
                    }

                    attributes.Add(values);
                }

                //A4AC0B27-5103-4E6C-A6E5-111BA1500F26 - Text ability
                //ae3e7551-1f43-4784-aec1-6771b7ddd018 - Text Header ability

                card.SetValue("displayName", model.Name);
                card.SetValue("set", Udi.Create(Constants.UdiEntityType.Document, set.Key).ToString());

                if (card.ContentType.Alias == Card.ModelTypeAlias)
                {
                    card.SetValue("hideFromDecks", model.HideFromDecks);
                }
                else if (model.VariantTypeId.HasValue)
                {
                    var variant = variants.FirstOrDefault(it => it.InternalID == model.VariantTypeId);
                    if (variant is not null)
                    {
                        card.SetValue("variantType", Udi.Create(Constants.UdiEntityType.Document, variant.Key).ToString());
                    }
                }
                if (model.ImageId.HasValue)
                {
                    var mediaItem = ctx.UmbracoContext.Media?.GetById(model.ImageId.Value);
                    if (mediaItem != null)
                    {
                        card.SetValue("image", Udi.Create(Constants.UdiEntityType.Media, mediaItem.Key).ToString());
                    }
                }
                if (model.BackImageId.HasValue)
                {
                    var mediaItem = ctx.UmbracoContext.Media?.GetById(model.BackImageId.Value);
                    if (mediaItem != null)
                    {
                        card.SetValue("backImage", Udi.Create(Constants.UdiEntityType.Media, mediaItem.Key).ToString());
                    }
                }

                card.SetValue("attributes", BlockListCreatorHelper.GetBlockListJsonFor(attributes!, new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26")));

                _contentService.SaveAndPublish(card);
            }
        }
    }

    public class ImportModel
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public int? VariantTypeId { get; set; }
        public int? ImageId { get; set; }
        public int? BackImageId { get; set; }
        public string Name { get; set; }
        public string SetName { get; set; }
        public bool HideFromDecks { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public ImportModel(int? id, string name, string setName)
        {
            Id = id;
            Name = name;
            SetName = setName;
            Properties = new Dictionary<string, string>();
        }

        public ImportModel(int? id, string name, string setName, Dictionary<string, string> properties)
        {
            Id = id;
            Name = name;
            SetName = setName;
            Properties = properties;
        }
    }
}
