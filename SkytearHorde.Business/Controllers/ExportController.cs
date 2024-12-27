using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfSharpCore;
using SkytearHorde.Business.Exports;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Controllers
{
    public class ExportController : UmbracoApiController
    {
        private readonly DeckService _deckService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly MemberInfoService _memberInfoService;
        private readonly ISiteService _siteService;
        private readonly ILogger<ExportController> _logger;
        private readonly CardService _cardService;
        private readonly SettingsService _settingsService;

        public ExportController(DeckService deckService,
            IWebHostEnvironment webHostEnvironment,
            MemberInfoService memberInfoService,
            ISiteService siteService,
            ILogger<ExportController> logger,
            CardService cardService,
            SettingsService settingsService)
        {
            _deckService = deckService;
            _webHostEnvironment = webHostEnvironment;
            _memberInfoService = memberInfoService;
            _siteService = siteService;
            _logger = logger;
            _cardService = cardService;
            _settingsService = settingsService;
        }

        public async Task<IActionResult> Export(int deckId, Guid exportId)
        {
            if (!TryGetDeck(deckId, out var deck))
            {
                return NotFound();
            }

            var exportType = GetExportTypes().FirstOrDefault(it => it.Key == exportId);
            if (exportType is null) { return NotFound(); }

            var exporter = GetExporter(exportType, deck);
            try
            {
                var bytes = await exporter.ExportDeck(deck);
                if (exportType.GetMimeType().Equals("redirect"))
                {
                    return Redirect(Encoding.UTF8.GetString(bytes));
                }

                var fileName = exportType.GetFileName(deck);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return File(bytes, exportType.GetMimeType());
                }
                return File(bytes, exportType.GetMimeType(), exportType.GetFileName(deck));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not export {deckId} with {nameof(exporter)}");
                throw;
            }
        }

        public async Task<IActionResult> ExportForceTable(int deckId)
        {
            if (!TryGetDeck(deckId, out var deck))
            {
                return NotFound();
            }

            var ttsExport = new SWUTTSExport(_cardService);
            var exportId = Convert.ToBase64String(await ttsExport.ExportDeck(deck));
            return Redirect($"https://www.forcetable.net/swu/import?svc=swuunlimiteddb&id={exportId}");
        }

        private bool TryGetDeck(int deckId, [NotNullWhen(true)] out Deck? deck)
        {
            deck = _deckService.Get(deckId, DeckStatus.None);
            if (deck is null) return false;

            var currentMember = _memberInfoService.GetMemberInfo();
            if (!deck.IsPublished && (currentMember is null || currentMember.Id != deck.CreatedBy))
            {
                deck = _deckService.Get(deckId, DeckStatus.Published);
                if (deck is null) return false;
            }
            return true;
        }

        private IEnumerable<IDeckExportType> GetExportTypes()
        {
            foreach(var deckOverview in _siteService.GetDeckOverviews())
            {
                var deckDetail = deckOverview.FirstChild<DeckDetail>();
                if (deckDetail is null)
                    continue;

                foreach (var type in deckDetail.ExportTypes.ToItems<IDeckExportType>())
                {
                    if (type is DeckExportGroup group)
                    {
                        foreach (var item in group.Items.ToItems<DeckExportGroupItem>())
                        {
                            yield return item.ExportType.ToItems<IDeckExportType>().First();
                        }
                        continue;
                    }

                    yield return type;
                }
            }
        }

        private IDeckExport GetExporter(IDeckExportType type, Deck deck)
        {
            if (type is DeckPdfExport pdfExportConfig)
            {
                var pageSize = pdfExportConfig.Size?.Equals("Letter") is true ? PageSize.Letter : PageSize.A4;
                return new PdfDeckExport(_webHostEnvironment, pageSize, _cardService);
            }
            if (type is DeckTtsexport ttsConfig)
            {
                return new TTSExport(_cardService, new TTSExportConfig(ttsConfig.Backimage?.Url(mode: UrlMode.Absolute))
                {
                    Width = ttsConfig.Width == 0 ? null : (float?)ttsConfig.Width,
                    Height = ttsConfig.Height == 0 ? null : (float?)ttsConfig.Height,
                    OrderDescending = ttsConfig.OrderDescending
                });
            }
            if (type is DeckSwuttsexport)
            {
                return new SWUTTSExport(_cardService);
            }
            if (type is DeckImageExport imageExport)
            {
                var colors = _deckService.GetColorsByDeck(deck).Select(it => it.Key).ToArray();
                return new ImageExport(_webHostEnvironment, _cardService, _siteService, new ImageExportConfig(_settingsService.GetSquadSettings(deck.TypeId))
                {
                    SortOptions = imageExport.Sorting.ToItems<SortOption>().ToArray(),
                    MainCardLogic = imageExport.MainCardLogic.ToItems<ISquadRequirementConfig>().ToArray(),
                    ShowCardAmounts = imageExport.ShowCardAmounts,
                }, colors);
            }
            if (type is DeckSptexport)
            {
                return new SPTExport(_cardService);
            }
            if (type is DeckRedirectExport redirectExport)
            {
                return new RedirectExport(redirectExport.RedirectUrl);
            }
            throw new NotImplementedException();
        }
    }
}
