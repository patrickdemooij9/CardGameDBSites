using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Business.Services
{
    public class CardImportService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CardImportService> _logger;
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardService _cardService;
        private readonly ISiteService _siteService;

        private const string ExternalApiBaseUrl = "https://api.sw-unlimited-db.com";

        public CardImportService(
            HttpClient httpClient,
            ILogger<CardImportService> logger,
            IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory,
            CardService cardService,
            ISiteService siteService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
            _siteService = siteService;
        }

        public async Task<CardImportResult> ImportCardsFromExternalApi()
        {
            var result = new CardImportResult();

            try
            {
                _logger.LogInformation("Starting card import from external API: {ApiUrl}", ExternalApiBaseUrl);

                // Fetch cards from external API
                var externalCards = await FetchCardsFromExternalApi();
                result.TotalCardsFetched = externalCards.Count;

                if (externalCards.Count == 0)
                {
                    _logger.LogWarning("No cards fetched from external API");
                    result.Message = "No cards found in external API";
                    return result;
                }

                // Get the root data node for cards
                var root = _siteService.GetRoot();
                var dataNode = root?.FirstChild<Data>();
                if (dataNode == null)
                {
                    _logger.LogError("Could not find Data node in site structure");
                    result.Message = "Site structure not properly configured (missing Data node)";
                    return result;
                }

                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                var publishedContentCache = ctx.UmbracoContext.Content;

                // Get all existing cards by name for quick lookup
                var existingCardsByName = GetExistingCardNames();

                // Process each card
                foreach (var externalCard in externalCards)
                {
                    try
                    {
                        var cardName = externalCard.Name ?? externalCard.DisplayName;
                        if (string.IsNullOrWhiteSpace(cardName))
                        {
                            _logger.LogWarning("Skipping card with no name");
                            result.CardsSkipped++;
                            continue;
                        }

                        // Check if card already exists
                        if (existingCardsByName.Contains(cardName))
                        {
                            _logger.LogDebug("Card '{CardName}' already exists, skipping", cardName);
                            result.CardsDuplicate++;
                            continue;
                        }

                        // Create new card
                        var newCard = _contentService.CreateAndSave(
                            cardName,
                            dataNode.Id,
                            Card.ModelTypeAlias);

                        if (newCard != null)
                        {
                            // Set card properties
                            SetCardProperties(newCard, externalCard);

                            // Save and publish
                            _contentService.SaveAndPublish(newCard);
                            result.CardsCreated++;
                            _logger.LogInformation("Created card: {CardName}", cardName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing card: {CardName}", externalCard.Name ?? externalCard.DisplayName);
                        result.CardsError++;
                    }
                }

                result.Message = $"Import completed. Created: {result.CardsCreated}, Duplicates: {result.CardsDuplicate}, Errors: {result.CardsError}";
                _logger.LogInformation("Card import completed: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during card import from external API");
                result.Message = $"Import failed: {ex.Message}";
            }

            return result;
        }

        private async Task<List<ExternalCardDto>> FetchCardsFromExternalApi()
        {
            var cards = new List<ExternalCardDto>();

            try
            {
                // Try fetching from /api/cards endpoint
                var response = await _httpClient.GetAsync($"{ExternalApiBaseUrl}/api/cards");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        // First try to parse as a direct array of cards
                        var cardArray = JsonSerializer.Deserialize<ExternalCardDto[]>(jsonContent);
                        if (cardArray != null && cardArray.Length > 0)
                        {
                            cards = cardArray.ToList();
                        }
                        else
                        {
                            // Try parsing as an object with a "cards" property
                            using var doc = JsonDocument.Parse(jsonContent);
                            var root = doc.RootElement;
                            
                            if (root.TryGetProperty("cards", out var cardsProperty))
                            {
                                var cardsJson = cardsProperty.GetRawText();
                                var parsedCards = JsonSerializer.Deserialize<ExternalCardDto[]>(cardsJson);
                                if (parsedCards != null)
                                {
                                    cards = parsedCards.ToList();
                                }
                            }
                            else if (root.ValueKind == JsonValueKind.Array)
                            {
                                // If root is an array, try to parse it
                                var parsedCards = JsonSerializer.Deserialize<ExternalCardDto[]>(jsonContent);
                                if (parsedCards != null)
                                {
                                    cards = parsedCards.ToList();
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing JSON from external API");
                    }
                }
                else
                {
                    _logger.LogError("Failed to fetch cards from external API. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching cards from external API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cards from external API");
            }

            return cards;
        }

        private HashSet<string> GetExistingCardNames()
        {
            try
            {
                var existingCards = _cardService.GetAll(includeVariants: false);
                return new HashSet<string>(
                    existingCards
                        .Where(c => !string.IsNullOrWhiteSpace(c.DisplayName))
                        .Select(c => c.DisplayName)
                        .Distinct(),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving existing card names");
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private void SetCardProperties(IContent cardContent, ExternalCardDto externalCard)
        {
            // Set the card name/displayName (usually the Name property in Umbraco)
            if (!string.IsNullOrWhiteSpace(externalCard.DisplayName))
            {
                cardContent.SetValue("displayName", externalCard.DisplayName);
            }

            // Set other properties if they exist in the external card
            if (!string.IsNullOrWhiteSpace(externalCard.SetCode))
            {
                // Note: You might need to resolve the set reference from the external API data
                // For now, we'll just store the set code as a property if it exists
                cardContent.SetValue("setCode", externalCard.SetCode);
            }

            if (!string.IsNullOrWhiteSpace(externalCard.CardType))
            {
                cardContent.SetValue("cardType", externalCard.CardType);
            }
        }
    }

    public class ExternalCardDto
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? SetCode { get; set; }
        public string? CardType { get; set; }
    }

    public class CardImportResult
    {
        public int TotalCardsFetched { get; set; }
        public int CardsCreated { get; set; }
        public int CardsDuplicate { get; set; }
        public int CardsSkipped { get; set; }
        public int CardsError { get; set; }
        public string Message { get; set; } = "Import started";
    }
}
