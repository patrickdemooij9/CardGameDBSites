using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HdbscanSharp.Runner;

const string ApiBaseUrl = "https://api.sw-unlimited-db.com";
const int DefaultMaxDecks = 200;
const int PageSize = 50;
const int MinClusterSize = 3;
const int MinPoints = 2;

Console.WriteLine("=== Deck Archetype Clustering (HDBSCAN) ===");
Console.WriteLine();

// Get max decks from user or use default
Console.Write($"Enter maximum number of decks to fetch (default: {DefaultMaxDecks}): ");
var input = Console.ReadLine();
int maxDecks = int.TryParse(input, out var parsed) && parsed > 0 ? parsed : DefaultMaxDecks;

Console.WriteLine($"Fetching up to {maxDecks} decks from {ApiBaseUrl}...");
Console.WriteLine();

// Fetch decks from the API
var decks = await FetchDecksAsync(maxDecks);
if (decks.Count == 0)
{
    Console.WriteLine("No decks were fetched. Exiting.");
    return;
}

Console.WriteLine($"Successfully fetched {decks.Count} decks.");
Console.WriteLine();

// Build feature vectors (card composition as a sparse vector)
Console.WriteLine("Building feature vectors from deck card compositions...");
var (featureVectors, cardIdIndex) = BuildFeatureVectors(decks);
Console.WriteLine($"Feature vector dimension: {cardIdIndex.Count} unique cards");
Console.WriteLine();

// Build distance function using cosine distance
Func<int, int, double> distanceFunc = (indexA, indexB) =>
{
    var a = featureVectors[indexA];
    var b = featureVectors[indexB];
    double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;

    for (int i = 0; i < a.Length; i++)
    {
        dotProduct += a[i] * b[i];
        magnitudeA += a[i] * a[i];
        magnitudeB += b[i] * b[i];
    }

    if (magnitudeA == 0 || magnitudeB == 0)
        return 1.0;

    var similarity = dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    return 1.0 - similarity;
};

// Run HDBSCAN clustering
Console.WriteLine("Running HDBSCAN clustering...");
var result = HdbscanRunner.Run(
    datasetCount: featureVectors.Length,
    minPoints: MinPoints,
    minClusterSize: MinClusterSize,
    distanceFunc: distanceFunc,
    constraints: null!);

// Group decks by cluster
var clusterGroups = new Dictionary<int, List<int>>();
for (int i = 0; i < result.Labels.Length; i++)
{
    var label = result.Labels[i];
    if (!clusterGroups.ContainsKey(label))
        clusterGroups[label] = new List<int>();
    clusterGroups[label].Add(i);
}

int noiseCount = clusterGroups.ContainsKey(0) ? clusterGroups[0].Count : 0;
int archetypeCount = clusterGroups.Keys.Count(k => k > 0);

Console.WriteLine();
Console.WriteLine($"=== Results ===");
Console.WriteLine($"Total decks analyzed: {decks.Count}");
Console.WriteLine($"Archetypes found: {archetypeCount}");
Console.WriteLine($"Noise/unclustered decks: {noiseCount}");
Console.WriteLine();

// Interactive exploration
while (true)
{
    Console.WriteLine("--- Archetype Overview ---");
    foreach (var cluster in clusterGroups.OrderBy(c => c.Key))
    {
        var label = cluster.Key == 0 ? "Noise (unclustered)" : $"Archetype {cluster.Key}";
        Console.WriteLine($"  [{cluster.Key}] {label}: {cluster.Value.Count} decks");
    }
    Console.WriteLine();
    Console.Write("Enter an archetype number to view its decks (or 'q' to quit): ");
    var selection = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(selection) || selection.Equals("q", StringComparison.OrdinalIgnoreCase))
        break;

    if (!int.TryParse(selection, out var clusterId) || !clusterGroups.ContainsKey(clusterId))
    {
        Console.WriteLine("Invalid selection. Please try again.");
        Console.WriteLine();
        continue;
    }

    var deckIndices = clusterGroups[clusterId];
    var clusterLabel = clusterId == 0 ? "Noise (unclustered)" : $"Archetype {clusterId}";
    Console.WriteLine();
    Console.WriteLine($"=== {clusterLabel} ({deckIndices.Count} decks) ===");

    // Find common cards in this cluster
    var cardCounts = new Dictionary<int, int>();
    foreach (var idx in deckIndices)
    {
        foreach (var card in decks[idx].Cards)
        {
            if (!cardCounts.ContainsKey(card.CardId))
                cardCounts[card.CardId] = 0;
            cardCounts[card.CardId]++;
        }
    }

    var commonCards = cardCounts
        .Where(c => c.Value >= deckIndices.Count * 0.5)
        .OrderByDescending(c => c.Value)
        .Take(10)
        .ToList();

    if (commonCards.Count > 0)
    {
        Console.WriteLine($"  Most common cards (in 50%+ of decks):");
        foreach (var card in commonCards)
        {
            var percentage = (double)card.Value / deckIndices.Count * 100;
            Console.WriteLine($"    Card ID {card.Key}: in {card.Value}/{deckIndices.Count} decks ({percentage:F0}%)");
        }
        Console.WriteLine();
    }

    Console.WriteLine("  Decks in this archetype:");
    foreach (var idx in deckIndices)
    {
        var deck = decks[idx];
        Console.WriteLine($"    - [{deck.Id}] \"{deck.Name}\" (Cards: {deck.Cards.Length}, Likes: {deck.AmountOfLikes})");
    }
    Console.WriteLine();
}

Console.WriteLine("Goodbye!");

// --- Helper methods ---

static async Task<List<DeckApiModel>> FetchDecksAsync(int maxDecks)
{
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(ApiBaseUrl);
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    var allDecks = new List<DeckApiModel>();
    int page = 1;
    int take = Math.Min(PageSize, maxDecks);

    while (allDecks.Count < maxDecks)
    {
        var query = new DeckQueryRequest
        {
            Take = take,
            Page = page,
            OrderBy = "newest"
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/decks/query", query);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PagedResult<DeckApiModel>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Items == null || result.Items.Length == 0)
                break;

            allDecks.AddRange(result.Items);
            Console.WriteLine($"  Fetched page {page}: {result.Items.Length} decks (total: {allDecks.Count}/{maxDecks})");

            if (allDecks.Count >= result.TotalItems || result.Items.Length < take)
                break;

            page++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error fetching page {page}: {ex.Message}");
            break;
        }
    }

    return allDecks.Take(maxDecks).ToList();
}

static (double[][] featureVectors, Dictionary<int, int> cardIdIndex) BuildFeatureVectors(List<DeckApiModel> decks)
{
    // Build an index of all unique card IDs
    var allCardIds = decks
        .SelectMany(d => d.Cards.Select(c => c.CardId))
        .Distinct()
        .OrderBy(id => id)
        .ToList();

    var cardIdIndex = new Dictionary<int, int>();
    for (int i = 0; i < allCardIds.Count; i++)
        cardIdIndex[allCardIds[i]] = i;

    // Build feature vectors (card amounts as features)
    var vectors = new double[decks.Count][];
    for (int i = 0; i < decks.Count; i++)
    {
        vectors[i] = new double[allCardIds.Count];
        foreach (var card in decks[i].Cards)
        {
            if (cardIdIndex.TryGetValue(card.CardId, out var idx))
                vectors[i][idx] = card.Amount;
        }
    }

    return (vectors, cardIdIndex);
}

// --- Models ---

public class DeckQueryRequest
{
    [JsonPropertyName("take")]
    public int Take { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; }

    [JsonPropertyName("cards")]
    public int[] Cards { get; set; } = Array.Empty<int>();
}

public class PagedResult<T>
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("items")]
    public T[]? Items { get; set; }
}

public class DeckApiModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("createdBy")]
    public int? CreatedBy { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonPropertyName("typeId")]
    public int TypeId { get; set; }

    [JsonPropertyName("amountOfLikes")]
    public int AmountOfLikes { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("cards")]
    public DeckCardApiModel[] Cards { get; set; } = Array.Empty<DeckCardApiModel>();
}

public class DeckCardApiModel
{
    [JsonPropertyName("cardId")]
    public int CardId { get; set; }

    [JsonPropertyName("groupId")]
    public int GroupId { get; set; }

    [JsonPropertyName("slotId")]
    public int SlotId { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}
