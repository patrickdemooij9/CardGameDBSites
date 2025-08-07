using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Exports.Collection;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System.IO;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Controllers
{
    public class CollectionController : UmbracoApiController
    {
        private readonly CardService _cardService;
        private readonly IMemberManager _memberManager;
        private readonly CollectionService _collectionService;
        private readonly ViewRenderHelper _viewRenderHelper;
        private readonly ISiteService _siteService;
        private readonly SettingsService _settingsService;
        private readonly ILogger<CollectionController> _logger;

        public CollectionController(CardService cardService,
            IMemberManager memberManager,
            CollectionService collectionService,
            ViewRenderHelper viewRenderHelper,
            ISiteService siteService,
            SettingsService settingsService,
            ILogger<CollectionController> logger)
        {
            _cardService = cardService;
            _memberManager = memberManager;
            _collectionService = collectionService;
            _viewRenderHelper = viewRenderHelper;
            _siteService = siteService;
            _settingsService = settingsService;
            _logger = logger;
        }

        public IActionResult RenderManageModal(int cardId, int setId)
        {
            if (!_memberManager.IsLoggedIn())
                return NotFound();

            var card = _cardService.Get(cardId);
            if (card is null)
                return NotFound();

            var variants = _collectionService.GetVariants(card, setId).ToArray();
            var variantTypes = _collectionService.GetVariantTypes();

            var itemViewModel = new CardItemViewModel(cardId)
            {
                SetId = card.SetId,
                Name = card.DisplayName,
                Variants = variants.Select(it => new CardVariantViewModel(it)).ToArray()
            };
            var collectionCards = _collectionService.GetCards().Where(it => it.CardId == cardId).ToArray();

            var collectionViewModel = new CollectionCardItemViewModel(itemViewModel)
            {
                Amounts = variants.ToDictionary(it => it.VariantId, it => collectionCards.FirstOrDefault(c => c.VariantId == it.VariantId)?.Amount ?? 0),
                VariantTypes = variantTypes.Select(it => new VariantTypeViewModel(it)).ToArray()
            };

            var manageModal = _viewRenderHelper.RenderView("~/Views/Partials/components/collectionCardManageModal.cshtml", collectionViewModel);
            return Content(manageModal.ToString());
        }

        public IActionResult AddSetToCollection(int setId)
        {
            if (!_memberManager.IsLoggedIn())
                return NotFound();

            _collectionService.AddSetToCollection(setId, 1);

            var result = _viewRenderHelper.RenderView("~/Views/Partials/components/collectionButton.cshtml", new CollectionButtonViewModel()
            {
                SetId = setId,
                ToAdd = false
            });
            var progressBar = _viewRenderHelper.RenderView("~/Views/Partials/components/progressBar.cshtml", new ProgressBarViewModel(_collectionService.CalculateCollectionProgress()));

            return new OkObjectResult(new[] {
                new AjaxResponse($"collection-{setId}", result.ToHtmlString()),
                new AjaxResponse("progress-bar", progressBar.ToHtmlString())
            });
        }

        public IActionResult RemoveSetFromCollection(int setId)
        {
            if (!_memberManager.IsLoggedIn())
                return NotFound();

            _collectionService.RemoveSetFromCollection(setId);

            var result = _viewRenderHelper.RenderView("~/Views/Partials/components/collectionButton.cshtml", new CollectionButtonViewModel()
            {
                SetId = setId,
                ToAdd = true
            });
            var progressBar = _viewRenderHelper.RenderView("~/Views/Partials/components/progressBar.cshtml", new ProgressBarViewModel(_collectionService.CalculateCollectionProgress()));

            return new OkObjectResult(new[] {
                new AjaxResponse($"collection-{setId}", result.ToHtmlString()),
                new AjaxResponse("progress-bar", progressBar.ToHtmlString())
            });
        }

        [HttpPost]
        public IActionResult UpdateSetInList(UpdateSetListPostModel postModel)
        {
            //TODO: Make everything work in a list mechanic
            if (postModel.ListName == "collection")
            {
                if (postModel.Value)
                {
                    _collectionService.AddSetToCollection(postModel.SetId, 1);
                }
                else
                {
                    _collectionService.RemoveSetFromCollection(postModel.SetId);
                }
                return Ok(new
                {
                    text = postModel.Value ? "Remove from collection" : "Add to collection"
                });
            }
            else if (postModel.ListName == "wishlist")
            {
                return Ok();
            }
            return NotFound();
        }

        public IActionResult UpdateCardCollection(int variantId, int amount)
        {
            var variant = _cardService.GetVariant(variantId);
            if (variant is null) return NotFound();

            _collectionService.UpdateCard(variant.BaseId, variantId, amount);

            return ReturnUpdatedResult(variant, true);
        }

        public IActionResult AddCardToCollection(int baseVariantId, [FromForm] Dictionary<int, int> variantAmounts)
        {
            var variant = _cardService.GetVariant(baseVariantId);
            if (variant is null) return NotFound();

            var variants = _collectionService.GetVariants(variant, variant.SetId).ToArray();
            if (variantAmounts.Keys.Any(it => !variants.Any(v => v.VariantId == it))) return NotFound();

            _collectionService.UpdateCard(variant.BaseId, variantAmounts);

            return ReturnUpdatedResult(variant, false);
        }

        private IActionResult ReturnUpdatedResult(Entities.Models.Business.Card variantCard, bool updateRow)
        {
            var variants = _cardService.GetVariantsForVariant(variantCard.VariantId);

            var collectionCards = _collectionService.GetCards().Where(it => it.CardId == variantCard.BaseId).ToArray();
            var result = new List<AjaxResponse>();

            var variantTypes = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();

            var itemViewModel = new CardItemViewModel(variantCard.BaseId)
            {
                SetId = variantCard.SetId,
                Name = variantCard.DisplayName,
                Variants = variants.Select(it => new CardVariantViewModel(it)).ToArray(),
                Collection = new CardCollectionViewModel()
                {
                    Amounts = variants.ToDictionary(it => it.VariantId, it => collectionCards.FirstOrDefault(c => c.VariantId == it.VariantId)?.Amount ?? 0),
                }
            };
            result.Add(new AjaxResponse($"total-count-{variantCard.BaseId}", itemViewModel.Collection.GetTotalAmount().ToString()));

            if (updateRow)
            {
                var dataRow = _viewRenderHelper.RenderView("~/Views/Partials/components/cardOverviewRow.cshtml", new CardOverviewRowDataModel(itemViewModel)
                {
                    AbilitiesToShow = new Dictionary<string, string> { { "Collection", "" } },
                    VariantTypes = variantTypes,
                    ShowCollection = true
                });
                result.Add(new AjaxResponse($"row-{variantCard.BaseId}", dataRow.ToHtmlString().ToString()));
            }

            return new OkObjectResult(result.ToArray());
        }

        public IActionResult VerifyPack([FromForm] PackPostModel postModel)
        {
            var sets = _cardService.GetAllSets().ToArray();
            var set = sets.FirstOrDefault(it => it.Id == postModel.SetId);
            if (set is null) return NotFound();

            var cards = _collectionService.ValidateCardsInPack(set.Id, postModel.Items, out var invalidItems);

            if (invalidItems.Count > 0)
            {
                var variantTypes = _collectionService.GetVariantTypes().Where(it => it.Identifier != null).ToArray();
                var modal = _viewRenderHelper.RenderView("~/Views/Partials/components/cardPackModal.cshtml", new PackViewModel
                {
                    Sets = sets.Select(it => new SetViewModel(it.Id, it.DisplayName ?? it.Name)).ToArray(),
                    VariantTypes = variantTypes,
                    ErrorMessage = $"Could not resolve items with ID: {string.Join(',', invalidItems.Select(it => it.Id))}",
                    PostContent = postModel
                });
                return new OkObjectResult(new[]
                {
                    new AjaxResponse($"modal-cardPack", modal.ToHtmlString())
                    {
                        IsInner = false
                    }
                });
            }

            var verifyModal = _viewRenderHelper.RenderView("~/Views/Partials/components/cardPackVerifyModal.cshtml", new VerifyPackViewModel
            {
                SetId = set.Id,
                Cards = cards,
                VariantTypes = _collectionService.GetVariantTypes().ToArray()
            });
            return new OkObjectResult(new[]
            {
                new AjaxResponse($"modal-cardPackVerify", verifyModal.ToHtmlString())
            });
        }

        [HttpPost]
        public IActionResult AddPreset(Guid presetId)
        {
            var collectionPage = _siteService.GetCollectionPage();
            if (collectionPage is null) return NotFound();

            var preset = collectionPage.Presets.ToItems<CardsPreset>().FirstOrDefault(it => it.Key == presetId);
            if (preset is null) return NotFound();

            foreach (var presetItem in preset.Items.ToItems<CardsPresetItem>())
            {
                var variant = presetItem.Card as CardVariant;
                if (presetItem.Card is Entities.Generated.Card card)
                {
                    variant = card.FirstChild<CardVariant>(it => it.VariantType is null);
                }

                if (variant is null) continue;

                _collectionService.AddCard(variant.Parent!.Id, variant.Id, presetItem.Amount);
            }

            return Redirect("/collection");
        }

        [HttpPost]
        public IActionResult AddPackToCollection([FromForm] PackPostModel postModel)
        {
            _collectionService.AddPack(postModel.SetId, postModel.Items);

            return Redirect("/collection"); // This is super ugly. I really need to figure out the backend/frontend situation
        }

        [HttpGet]
        public async Task<IActionResult> ExportCollection(CollectionExportType exportType)
        {
            var collection = _collectionService.GetCards();
            var variantTypes = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();

            byte[] data;
            if (exportType == CollectionExportType.Grouped)
            {
                data = await new ExcelCollectionManager(variantTypes, _cardService.GetAllSets().ToArray(), _cardService).Export(collection);
            }
            else
            {
                data = await new DetailedCollectionImport(variantTypes, _cardService, _settingsService.GetCollectionSettings().ImportMappings).Export(collection);
            }

            return File(data, "application/octet-stream", "CollectionExport.xlsx");
        }

        public async Task<IActionResult> ImportCollection(bool overwrite, CollectionExportType exportType)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file is null) return NotFound();

            var variants = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();
            var items = new List<CollectionCardItem>();

            var importFailed = false;
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".csv"))
                    {
                        items.AddRange(new CsvCollectionManager(variants, _cardService, _settingsService.GetCollectionSettings().ImportMappings).Import(stream));
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
                    {
                        items.AddRange(new ExcelCollectionManager(variants, _cardService.GetAllSets().ToArray(), _cardService).Import(stream));
                    }
                    else
                    {
                        return BadRequest("File should be an .xlsx or .csv (SWUDB format)");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            if (items.Count == 0)
            {
                return BadRequest("No rows found to import");
            }

            var collection = _collectionService.GetCards();
            if (overwrite)
            {
                _collectionService.ClearCollection();
            }
            try
            {
                foreach (var item in items)
                {
                    _collectionService.UpdateCard(item.CardId, item.VariantId, item.Amount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while importing");

                if (overwrite)
                {
                    foreach (var item in collection)
                    {
                        _collectionService.UpdateCard(item.CardId, item.VariantId, item.Amount);
                    }
                }
            }

            return Ok();
        }
    }
}
