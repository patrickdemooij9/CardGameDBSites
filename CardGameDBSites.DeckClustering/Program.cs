using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HdbscanSharp.Runner;

const string ApiBaseUrl = "https://api.sw-unlimited-db.com";
const int DefaultMaxDecks = 200;
const int PageSize = 50;
const int MinClusterSize = 3;
const int MinPoints = 2;
const int StandardDeckTypeId = 1;
const int MainGroupId = -1;
const double LeaderBaseWeightMultiplier = 3.0;

Console.WriteLine("=== Deck Archetype Clustering (HDBSCAN) ===");
Console.WriteLine();

// Get max decks from user or use default
Console.Write($"Enter maximum number of decks to fetch (default: {DefaultMaxDecks}): ");
var input = Console.ReadLine();
int maxDecks = int.TryParse(input, out var parsed) && parsed > 0 ? parsed : DefaultMaxDecks;

Console.WriteLine($"Fetching up to {maxDecks} standard decks from {ApiBaseUrl}...");
Console.WriteLine();

// Fetch decks from the API
var decks = await FetchDecksAsync(maxDecks);
if (decks.Count == 0)
{
    Console.WriteLine("No decks were fetched. Exiting.");
    return;
}

Console.WriteLine($"Successfully fetched {decks.Count} standard decks.");
Console.WriteLine();

// Build feature vectors with TF-IDF weighting and leader/base emphasis
Console.WriteLine("Building feature vectors (TF-IDF weighted, leader/base emphasized)...");
var (featureVectors, cardIdIndex, idfWeights, cardGroupMap) = BuildFeatureVectors(decks);
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

    // Find core cards that define this archetype
    // Core cards = cards that appear frequently in this cluster but rarely in other clusters (high TF-IDF relevance)
    var cardClusterFrequency = new Dictionary<int, int>();
    var cardClusterTotalAmount = new Dictionary<int, int>();
    foreach (var idx in deckIndices)
    {
        foreach (var card in decks[idx].Cards)
        {
            if (!cardClusterFrequency.ContainsKey(card.CardId))
            {
                cardClusterFrequency[card.CardId] = 0;
                cardClusterTotalAmount[card.CardId] = 0;
            }
            cardClusterFrequency[card.CardId]++;
            cardClusterTotalAmount[card.CardId] += card.Amount;
        }
    }

    // Calculate a "defining score" for each card: frequency in cluster * IDF weight
    // This highlights cards that are common in this archetype but rare globally
    var reverseCardIndex = cardIdIndex.ToDictionary(kv => kv.Value, kv => kv.Key);
    var coreCards = cardClusterFrequency
        .Select(kv =>
        {
            var cardId = kv.Key;
            var clusterFreq = (double)kv.Value / deckIndices.Count;
            var idfWeight = cardIdIndex.ContainsKey(cardId) ? idfWeights[cardIdIndex[cardId]] : 1.0;
            var isLeaderBase = cardGroupMap.ContainsKey(cardId) && cardGroupMap[cardId] != MainGroupId;
            var definingScore = clusterFreq * idfWeight * (isLeaderBase ? LeaderBaseWeightMultiplier : 1.0);
            return new { CardId = cardId, ClusterFreq = clusterFreq, DefiningScore = definingScore, IsLeaderBase = isLeaderBase, Count = kv.Value };
        })
        .Where(c => c.ClusterFreq >= 0.4) // Present in at least 40% of decks in this archetype
        .OrderByDescending(c => c.DefiningScore)
        .Take(15)
        .ToList();

    if (coreCards.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"  Core cards defining this archetype:");
        foreach (var card in coreCards)
        {
            var percentage = card.ClusterFreq * 100;
            var tag = card.IsLeaderBase ? " [LEADER/BASE]" : "";
            Console.WriteLine($"    Card ID {card.CardId}{tag}: in {card.Count}/{deckIndices.Count} decks ({percentage:F0}%), defining score: {card.DefiningScore:F2}");
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
            OrderBy = "newest",
            TypeId = StandardDeckTypeId
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

static (double[][] featureVectors, Dictionary<int, int> cardIdIndex, double[] idfWeights, Dictionary<int, int> cardGroupMap) BuildFeatureVectors(List<DeckApiModel> decks)
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

    // Build a map of cardId -> groupId (use the first occurrence)
    var cardGroupMap = new Dictionary<int, int>();
    foreach (var deck in decks)
    {
        foreach (var card in deck.Cards)
        {
            if (!cardGroupMap.ContainsKey(card.CardId))
                cardGroupMap[card.CardId] = card.GroupId;
        }
    }

    // Calculate IDF (Inverse Document Frequency) for each card
    // IDF = log(totalDecks / numberOfDecksContainingCard)
    int totalDecks = decks.Count;
    var documentFrequency = new int[allCardIds.Count];
    for (int i = 0; i < decks.Count; i++)
    {
        var cardsInDeck = decks[i].Cards.Select(c => c.CardId).Distinct();
        foreach (var cardId in cardsInDeck)
        {
            if (cardIdIndex.TryGetValue(cardId, out var idx))
                documentFrequency[idx]++;
        }
    }

    var idfWeights = new double[allCardIds.Count];
    for (int i = 0; i < allCardIds.Count; i++)
    {
        idfWeights[i] = documentFrequency[i] > 0
            ? Math.Log((double)totalDecks / documentFrequency[i])
            : 0;
    }

    // Build feature vectors with TF-IDF weighting and leader/base weight multiplier
    var vectors = new double[decks.Count][];
    for (int i = 0; i < decks.Count; i++)
    {
        vectors[i] = new double[allCardIds.Count];
        foreach (var card in decks[i].Cards)
        {
            if (cardIdIndex.TryGetValue(card.CardId, out var idx))
            {
                // TF = card amount (term frequency in this deck)
                double tf = card.Amount;

                // Apply TF-IDF weighting
                double tfidf = tf * idfWeights[idx];

                // Apply higher weight for leader/base cards (non-main-group)
                if (card.GroupId != MainGroupId)
                    tfidf *= LeaderBaseWeightMultiplier;

                vectors[i][idx] = tfidf;
            }
        }
    }

    return (vectors, cardIdIndex, idfWeights, cardGroupMap);
}

// --- Models ---

public class DeckQueryRequest
{
    [JsonPropertyName("typeId")]
    public int? TypeId { get; set; }

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
