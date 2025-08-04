using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPoco.Expressions;
using OfficeOpenXml;
using SkytearHorde.Business.Config;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Integrations.TcgPlayer;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels;
using System.IO.Compression;
using System.Net.Http.Headers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using static Lucene.Net.Search.FieldValueHitQueue;
using static SkytearHorde.Business.Helpers.BlockListCreatorHelper;
using static Umbraco.Cms.Core.Collections.TopoGraph;

namespace SkytearHorde.Modules
{
    [PluginController("Importer")]
    public class ImporterController : UmbracoAuthorizedApiController
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly CardService _cardService;
        private readonly MediaFileManager _mediaFileManager;
        private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ISiteAccessor _siteAccessor;
        private readonly ISiteService _siteService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly CardGameSettingsConfig _cardGameSettingsConfig;
        private readonly CollectionService _collectionService;
        private readonly ILogger<ImporterController> _logger;

        public ImporterController(IUmbracoContextFactory umbracoContextFactory,
            IContentService contentService,
            IMediaService mediaService,
            CardService cardService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            ISiteAccessor siteAccessor,
            ISiteService siteService,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IOptions<CardGameSettingsConfig> cardGameSettingsConfigOption,
            CollectionService collectionService,
            ILogger<ImporterController> logger)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
            _mediaService = mediaService;
            _cardService = cardService;
            _mediaFileManager = mediaFileManager;
            _mediaUrlGenerators = mediaUrlGenerators;
            _shortStringHelper = shortStringHelper;
            _siteAccessor = siteAccessor;
            _siteService = siteService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _cardGameSettingsConfig = cardGameSettingsConfigOption.Value;
            _collectionService = collectionService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Import(int nodeId)
        {
            _siteAccessor.SetSiteId(GetSiteIdByNode(nodeId));
            var file = Request.Form.Files.FirstOrDefault();
            var importModels = ReadExcel(file.OpenReadStream());
            ImportModels(importModels);

            return Ok();
        }

        public IActionResult ImportJsonFiles(int nodeId, int setId)
        {
            _siteAccessor.SetSiteId(GetSiteIdByNode(nodeId));

            using (var reader = new StreamReader(Request.Form.Files.FirstOrDefault()!.OpenReadStream()))
            using (var jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                var files = ser.Deserialize<CardReaderOuputItem[]>(jsonReader) ?? [];

                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                var cardImageParentId = _siteService.GetSettings().FirstChild<SiteSettings>().CardImageRoot.Id;
                var setName = ctx.UmbracoContext.Content.GetById(setId).Name;

                var importModels = new List<ImportModel>();
                foreach (var item in files)
                {
                    var itemName = item.Name;
                    if (item.TryGetValue("Subname", out var subname))
                    {
                        itemName += $", {subname}";
                    }

                    var media = _mediaService.CreateMediaWithIdentity(itemName, cardImageParentId, Image.ModelTypeAlias);

                    using var mediaItemStream = new MemoryStream(Convert.FromBase64String(item.ImageBase64));
                    media.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, "umbracoFile", itemName + ".png", mediaItemStream);
                    _mediaService.Save(media);

                    var properties = new Dictionary<string, string>();
                    var ignoredProperties = new string[] { "Name", "image", "image_base64" };
                    foreach (var entry in item.Where(it => !ignoredProperties.Contains(it.Key, StringComparer.InvariantCultureIgnoreCase)))
                    {
                        properties.Add(entry.Key, entry.Value.ToString()!);
                    }
                    importModels.Add(new ImportModel(null, itemName, setName)
                    {
                        ImageId = media.Id,
                        Properties = properties
                    });
                }

                ImportModels(importModels);
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult ImportImages(IFormFile file, int nodeId)
        {
            var zipArchive = new ZipArchive(file.OpenReadStream());

            _siteAccessor.SetSiteId(GetSiteIdByNode(nodeId));

            var cards = _cardService.GetAll();

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var cardImageParentId = _siteService.GetSettings().FirstChild<SiteSettings>().CardImageRoot.Id;

            foreach (var entry in zipArchive.Entries)
            {
                var fileName = Path.GetFileNameWithoutExtension(entry.Name);
                var isBackImage = fileName.EndsWith("_back");
                if (isBackImage)
                {
                    fileName = fileName.TrimEnd("_back");
                }

                var card = cards.FirstOrDefault(it => it.DisplayName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                if (card is null) continue;

                var umbracoCard = ctx.UmbracoContext.Content.GetById(card.BaseId);
                if (umbracoCard is null) continue;

                var imageAlias = isBackImage ? "backImage" : "image";

                IMedia media;
                if (umbracoCard.Value<MediaWithCrops>(imageAlias) is null)
                    media = _mediaService.CreateMediaWithIdentity(fileName, cardImageParentId, Image.ModelTypeAlias);
                else
                    media = _mediaService.GetById(umbracoCard.Value<MediaWithCrops>(imageAlias).Id)!;

                media.SetValue(_mediaFileManager, _mediaUrlGenerators, _shortStringHelper, _contentTypeBaseServiceProvider, "umbracoFile", entry.Name, entry.Open());
                _mediaService.Save(media);

                if (umbracoCard.Value<MediaWithCrops>(imageAlias) is null)
                {
                    var cardContent = _contentService.GetById(card.BaseId)!;
                    cardContent.SetValue(imageAlias, Udi.Create(Constants.UdiEntityType.Media, media.Key).ToString());
                    _contentService.SaveAndPublish(cardContent);
                }
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ImportPricesFromTcgPlayer(int siteId, int setId, int amount)
        {
            _siteAccessor.SetSiteId(siteId);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            if (_siteService.GetSettings().FirstChild<SiteSettings>()?.AllowPricingSync != true)
            {
                return NotFound();
            }

            var dataContainer = _siteService.GetRoot().FirstChild<Data>();
            var cardContainer = dataContainer?.FirstChild<CardContainer>();
            if (cardContainer is null) return NotFound();

            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Id == setId);
            if (set is null) return NotFound();

            var variants = _cardService.GetAllBySet(setId, true).Where(it => it.VariantId != 0 && it.GetMultipleCardAttributeValue("TcgPlayerId")?.Any() != true).ToArray();
            var variantTypes = _collectionService.GetVariantTypes().ToDictionary(it => it.Id, it => it);

            if (variants.Length == 0) return NotFound();

            var tcgAttribute = dataContainer?.FirstChild<CardAttributeContainer>()?.FirstChild<CardAttribute>(it => it.Name.Equals("TcgPlayerId"));
            if (tcgAttribute is null) return NotFound();

            try
            {
                var httpClient = new HttpClient();
                var accessToken = await new TcgPlayerAccessTokenGetter().Get(_cardGameSettingsConfig, httpClient);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                foreach (var variant in variants.Take(amount))
                {
                    var cleanedDisplayName = variant.DisplayName.Replace(",", "").Replace("'", "").Replace("-", " ");
                    var namesToSearch = new List<string>() { cleanedDisplayName };
                    var subNames = variant.GetMultipleCardAttributeValue("Subname");
                    if (subNames?.Any() is true && !string.IsNullOrWhiteSpace(subNames[0]))
                    {
                        namesToSearch.Add(cleanedDisplayName.Replace(subNames[0], "").Trim());
                    }

                    var variantName = "";
                    if (variant.VariantTypeId.HasValue)
                    {
                        variantName = variantTypes[variant.VariantTypeId.Value].DisplayName;
                        namesToSearch = namesToSearch.Select(it => $"{it} {variantName}").ToList();
                    }

                    var idsFound = new List<long>();
                    foreach (var name in namesToSearch)
                    {
                        _logger.LogInformation($"Searching for {name}");
                        var result = await httpClient.PostAsJsonAsync("https://api.tcgplayer.com/catalog/categories/79/search", new TcgPlayerSearchModel
                        {
                            Limit = 5,
                            Filters = [new TcgPlayerSearchFilterModel {
                        Name = "ProductName",
                        Values = [name]
                    }]
                        });
                        if (!result.IsSuccessStatusCode) continue;

                        var resultModel = await result.Content.ReadFromJsonAsync<TcgPlayerResult<long>>();
                        idsFound.AddRange(resultModel?.Results ?? Enumerable.Empty<long>());
                    }

                    if (idsFound.Count == 0) continue;

                    var detailedResponse = await httpClient.GetAsync($"https://api.tcgplayer.com/catalog/products/{string.Join(",", idsFound.Distinct())}");
                    if (!detailedResponse.IsSuccessStatusCode) continue;

                    var detailedResult = await detailedResponse.Content.ReadFromJsonAsync<TcgPlayerResult<TcgPlayerProductModel>>();
                    var matchFound = detailedResult?.Results.FirstOrDefault(it =>
                    {
                        if (it.GroupId != set.TcgPlayerCategory)
                        {
                            return false;
                        }

                        if (variant.VariantTypeId.HasValue)
                        {
                            return it.CleanName.Contains(variantName, StringComparison.InvariantCultureIgnoreCase);
                        }
                        else
                        {
                            return variantTypes.Select(v => v.Value.DisplayName).All(v => !it.CleanName.Contains(v, StringComparison.InvariantCultureIgnoreCase));
                        }
                    });
                    if (matchFound != null)
                    {
                        var contentItem = _contentService.GetById(variant.VariantId);
                        if (contentItem is null) continue;

                        var attributeJson = contentItem.GetValue<string>("attributes");
                        var values = new Dictionary<string, string>()
                    {
                        {"ability", Udi.Create(Constants.UdiEntityType.Document, tcgAttribute.Key).ToString() }, {"contentTypeKey", "A4AC0B27-5103-4E6C-A6E5-111BA1500F26"},
                        {"value", matchFound.ProductId.ToString() }
                    };

                        if (string.IsNullOrWhiteSpace(attributeJson))
                        {
                            contentItem.SetValue("attributes", BlockListCreatorHelper.GetBlockListJsonFor(new List<Dictionary<string, string>>
                        {
                            values
                        }, new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26")));
                            _contentService.SaveAndPublish(contentItem);
                            continue;
                        }

                        var attributes = JsonConvert.DeserializeObject<Blocklist>(attributeJson);
                        var udi = new GuidUdi("element", Guid.NewGuid()).ToString();
                        values.Add("udi", udi);
                        attributes.Layout.ContentUdi.Add(new Dictionary<string, string> { { "contentUdi", udi } });
                        attributes.ContentData.Add(values);

                        contentItem.SetValue("attributes", JsonConvert.SerializeObject(attributes));
                        _contentService.SaveAndPublish(contentItem);

                        //Woohoo
                    }
                    else
                    {
                        _logger.LogWarning("Could not find match for " + variant.VariantId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong");
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult Export(int siteId)
        {
            _siteAccessor.SetSiteId(siteId);

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var parentContainer = _siteService.GetRoot().FirstChild<Data>();
            var cardsContainer = parentContainer?.FirstChild<CardContainer>();
            var dataContainer = parentContainer?.FirstChild<SetContainer>();
            var attributeContainer = parentContainer?.FirstChild<CardAttributeContainer>();

            var attributes = attributeContainer.Children<CardAttribute>().ToArray();
            var memoryStream = new MemoryStream();

            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Data");

                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "ParentId";
                worksheet.Cells[1, 3].Value = "Name";
                worksheet.Cells[1, 4].Value = "Set Name";
                worksheet.Cells[1, 5].Value = "Hide from deck";
                worksheet.Cells[1, 6].Value = "Variant Type Id";

                for (var i = 0; i < attributes.Length; i++)
                {
                    var attributeName = attributes[i].Name;
                    if (attributes[i].IsMultiValue)
                    {
                        attributeName = $"{attributeName}_multiple";
                    }
                    worksheet.Cells[1, 7 + i].Value = attributeName;
                }

                var row = 2;

                var cards = dataContainer.Children<Set>().SelectMany(it => it.Descendants()).ToList();
                if (cardsContainer != null)
                {
                    cards.AddRange(cardsContainer.Descendants());
                }

                var sets = _cardService.GetAllSets();
                foreach (var item in cards)
                {
                    if (item is CardVariant variant)
                    {
                        var cardSet = (variant.Set as Set) ?? (variant.Parent as Card)?.Set?.FirstOrDefault();
                        if (cardSet is null) { continue; }

                        var cardAttributes = variant.Attributes.ToItems<IAbilityValue>().ToArray();

                        worksheet.Cells[row, 1].Value = variant.Id;
                        worksheet.Cells[row, 2].Value = variant.Parent!.Id;
                        worksheet.Cells[row, 3].Value = variant.DisplayName;
                        worksheet.Cells[row, 4].Value = cardSet.Name;
                        worksheet.Cells[row, 5].Value = false;
                        worksheet.Cells[row, 6].Value = (variant.VariantType as Variant)?.InternalID;

                        for (var i = 0; i < attributes.Length; i++)
                        {
                            var cardAttribute = cardAttributes.FirstOrDefault(it => it.Ability?.Id == attributes[i].Id);
                            if (cardAttribute is null) continue;

                            worksheet.Cells[row, 7 + i].Value = cardAttribute.GetAbilityValue();
                            worksheet.Cells[row, 7 + i].AutoFitColumns();
                        }
                    }
                    else if (item is Card card)
                    {
                        var cardSet = (card.Set?.OfType<Set>().FirstOrDefault()) ?? card.Parent as Set;
                        var cardAttributes = card.Attributes.ToItems<IAbilityValue>().ToArray();

                        worksheet.Cells[row, 1].Value = card.Id;
                        worksheet.Cells[row, 2].Value = null;
                        worksheet.Cells[row, 3].Value = card.DisplayName;
                        worksheet.Cells[row, 4].Value = cardSet.Name;
                        worksheet.Cells[row, 5].Value = card.HideFromDecks;
                        worksheet.Cells[row, 6].Value = null;

                        for (var i = 0; i < attributes.Length; i++)
                        {
                            var cardAttribute = cardAttributes.FirstOrDefault(it => it.Ability?.Id == attributes[i].Id);
                            if (cardAttribute is null) continue;

                            worksheet.Cells[row, 7 + i].Value = cardAttribute.GetAbilityValue();
                            worksheet.Cells[row, 7 + i].AutoFitColumns();
                        }
                    }

                    row++;
                }

                worksheet.Tables.Add(new ExcelAddressBase(1, 1, row - 1, attributes.Length + 6), "Cards");

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0;

            return File(memoryStream, "application/octet-stream", "CardList.xlsx");
        }

        private IEnumerable<ImportModel> ReadExcel(Stream stream)
        {
            var importModels = new List<ImportModel>();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var table = worksheet.Tables[0];

                var start = table.Address.Start;
                var end = table.Address.End;

                for (var r = start.Row + 1; r <= end.Row; r++)
                {
                    int? id = null;
                    int? parentId = null;
                    int? VariantTypeId = null;
                    string name = "";
                    string setName = "";
                    bool hideFromDeck = false;
                    Dictionary<string, string> properties = new Dictionary<string, string>();

                    for (var c = start.Column; c <= end.Column; c++)
                    {
                        var columnName = table.Columns[c - 1].Name;
                        var value = table.WorkSheet.Cells[r, c].Value;
                        if (value is null) continue;

                        if (columnName == "Id") id = Convert.ToInt32(value);
                        else if (columnName == "ParentId") parentId = value is null ? null : Convert.ToInt32(value);
                        else if (columnName == "Variant Type Id") VariantTypeId = value is null ? null : Convert.ToInt32(value);
                        else if (columnName == "Name") name = value.ToString();
                        else if (columnName == "Set Name") setName = value.ToString();
                        else if (columnName == "Hide from deck") hideFromDeck = bool.Parse(value.ToString());
                        else properties.Add(columnName, value.ToString());
                    }

                    importModels.Add(new ImportModel(id, name, setName, properties)
                    {
                        ParentId = parentId,
                        VariantTypeId = VariantTypeId,
                        HideFromDecks = hideFromDeck
                    });
                }
            }
            return importModels;
        }

        private void ImportModels(IEnumerable<ImportModel> models)
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

                card.SetValue("attributes", BlockListCreatorHelper.GetBlockListJsonFor(attributes!, new Guid("A4AC0B27-5103-4E6C-A6E5-111BA1500F26")));

                _contentService.SaveAndPublish(card);
            }
        }

        private int GetSiteIdByNode(int nodeId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var currentNode = ctx.UmbracoContext.Content.GetById(nodeId);
            return currentNode.Root().FirstChild<Settings>().FirstChild<SiteSettings>().SiteId;
        }
    }
}
