using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HdbscanSharp.Runner;

const string ApiBaseUrl = "https://api.sw-unlimited-db.com";
const int DefaultMaxDecks = 500;
const int PageSize = 50;
const int MinClusterSize = 3;
const int MinPoints = 2;
const int StandardDeckTypeId = 1;
const int LeaderSlotId = 1;

Console.WriteLine("=== Deck Archetype Clustering (HDBSCAN) ===");
Console.WriteLine("Approach: Binary vectors (1/0), clustered per leader");
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

// Group decks by leader
var leaderGroups = decks
    .GroupBy(d => d.Cards.FirstOrDefault(c => c.SlotId == LeaderSlotId)?.CardId ?? 0)
    .Where(g => g.Key != 0)
    .OrderByDescending(g => g.Count())
    .ToList();

Console.WriteLine($"Found {leaderGroups.Count} distinct leaders:");
foreach (var group in leaderGroups)
    Console.WriteLine($"  Leader {group.Key}: {group.Count()} decks");
Console.WriteLine();

// Run clustering per leader
var allResults = new List<LeaderClusterResult>();

foreach (var leaderGroup in leaderGroups)
{
    var leaderDecks = leaderGroup.ToList();
    var leaderId = leaderGroup.Key;

    if (leaderDecks.Count < MinClusterSize)
    {
        Console.WriteLine($"Leader {leaderId}: only {leaderDecks.Count} decks, skipping (need at least {MinClusterSize})");
        allResults.Add(new LeaderClusterResult(leaderId, leaderDecks, new Dictionary<int, List<int>>(), Array.Empty<double[]>()));
        continue;
    }

    // Build binary feature vectors: 1 if card is in deck, 0 otherwise
    // Exclude the leader card itself since all decks in this group share it
    var allCardIds = leaderDecks
        .SelectMany(d => d.Cards.Where(c => c.SlotId != LeaderSlotId).Select(c => c.CardId))
        .Distinct()
        .OrderBy(id => id)
        .ToList();

    var cardIdIndex = new Dictionary<int, int>();
    for (int i = 0; i < allCardIds.Count; i++)
        cardIdIndex[allCardIds[i]] = i;

    var vectors = new double[leaderDecks.Count][];
    for (int i = 0; i < leaderDecks.Count; i++)
    {
        vectors[i] = new double[allCardIds.Count];
        var deckCardIds = leaderDecks[i].Cards
            .Where(c => c.SlotId != LeaderSlotId)
            .Select(c => c.CardId)
            .Distinct();

        foreach (var cardId in deckCardIds)
        {
            if (cardIdIndex.TryGetValue(cardId, out var idx))
                vectors[i][idx] = 1.0;
        }
    }

    // Distance function: Jaccard distance (1 - Jaccard similarity)
    Func<int, int, double> distanceFunc = (indexA, indexB) =>
    {
        var a = vectors[indexA];
        var b = vectors[indexB];
        int shared = 0, total = 0;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] == 1.0 || b[i] == 1.0)
            {
                total++;
                if (a[i] == 1.0 && b[i] == 1.0)
                    shared++;
            }
        }

        if (total == 0) return 1.0;
        return 1.0 - (double)shared / total;
    };

    // Run HDBSCAN
    var result = HdbscanRunner.Run(
        datasetCount: vectors.Length,
        minPoints: MinPoints,
        minClusterSize: MinClusterSize,
        distanceFunc: distanceFunc,
        constraints: null!);

    // Group by cluster
    var clusterGroups = new Dictionary<int, List<int>>();
    for (int i = 0; i < result.Labels.Length; i++)
    {
        var label = result.Labels[i];
        if (!clusterGroups.ContainsKey(label))
            clusterGroups[label] = new List<int>();
        clusterGroups[label].Add(i);
    }

    allResults.Add(new LeaderClusterResult(leaderId, leaderDecks, clusterGroups, vectors));

    int noiseCount = clusterGroups.ContainsKey(0) ? clusterGroups[0].Count : 0;
    int archetypeCount = clusterGroups.Keys.Count(k => k > 0);
    Console.WriteLine($"Leader {leaderId}: {leaderDecks.Count} decks -> {archetypeCount} archetypes, {noiseCount} noise");
}

Console.WriteLine();
Console.WriteLine("=== Clustering Complete ===");
Console.WriteLine();

// Interactive exploration
while (true)
{
    Console.WriteLine("--- Leaders ---");
    for (int i = 0; i < allResults.Count; i++)
    {
        var r = allResults[i];
        var archetypes = r.ClusterGroups.Keys.Count(k => k > 0);
        var noise = r.ClusterGroups.ContainsKey(0) ? r.ClusterGroups[0].Count : 0;
        Console.WriteLine($"  [{i}] Leader {r.LeaderId}: {r.Decks.Count} decks, {archetypes} archetypes, {noise} noise");
    }
    Console.WriteLine();
    Console.Write("Enter leader index to explore (or 'q' to quit): ");
    var selection = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(selection) || selection.Equals("q", StringComparison.OrdinalIgnoreCase))
        break;

    if (!int.TryParse(selection, out var leaderIdx) || leaderIdx < 0 || leaderIdx >= allResults.Count)
    {
        Console.WriteLine("Invalid selection.");
        Console.WriteLine();
        continue;
    }

    var leaderResult = allResults[leaderIdx];
    Console.WriteLine();
    Console.WriteLine($"=== Leader {leaderResult.LeaderId} ({leaderResult.Decks.Count} decks) ===");

    if (leaderResult.ClusterGroups.Count == 0)
    {
        Console.WriteLine("  Too few decks for clustering.");
        Console.WriteLine();
        continue;
    }

    // Show archetypes for this leader
    foreach (var cluster in leaderResult.ClusterGroups.OrderBy(c => c.Key))
    {
        var label = cluster.Key == 0 ? "Noise (unclustered)" : $"Archetype {cluster.Key}";
        Console.WriteLine();
        Console.WriteLine($"  --- {label} ({cluster.Value.Count} decks) ---");

        // Find common cards (present in >50% of decks in this cluster)
        if (cluster.Value.Count > 0)
        {
            var cardFrequency = new Dictionary<int, int>();
            foreach (var deckIdx in cluster.Value)
            {
                foreach (var card in leaderResult.Decks[deckIdx].Cards.Where(c => c.SlotId != LeaderSlotId))
                {
                    if (!cardFrequency.ContainsKey(card.CardId))
                        cardFrequency[card.CardId] = 0;
                    cardFrequency[card.CardId]++;
                }
            }

            var coreCards = cardFrequency
                .Where(kv => (double)kv.Value / cluster.Value.Count >= 0.5)
                .OrderByDescending(kv => kv.Value)
                .Take(10)
                .ToList();

            if (coreCards.Count > 0)
            {
                Console.WriteLine("  Core cards (in 50%+ of decks):");
                foreach (var card in coreCards)
                {
                    var pct = (double)card.Value / cluster.Value.Count * 100;
                    Console.WriteLine($"    Card {card.Key}: {card.Value}/{cluster.Value.Count} ({pct:F0}%)");
                }
            }
        }

        // List decks
        Console.WriteLine("  Decks:");
        foreach (var deckIdx in cluster.Value)
        {
            var deck = leaderResult.Decks[deckIdx];
            Console.WriteLine($"    [{deck.Id}] \"{deck.Name}\" (Likes: {deck.AmountOfLikes})");
        }
    }
    Console.WriteLine();
}

Console.WriteLine("Goodbye!");

// === Helper methods ===

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

// === Models ===

public record LeaderClusterResult(int LeaderId, List<DeckApiModel> Decks, Dictionary<int, List<int>> ClusterGroups, double[][] Vectors);

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
