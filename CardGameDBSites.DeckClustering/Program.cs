using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HdbscanSharp.Runner;

// === Configuration ===
var config = new ClusteringConfig();

const string ApiBaseUrl = "https://api.sw-unlimited-db.com";
const int DefaultMaxDecks = 200;
const int PageSize = 50;
const int StandardDeckTypeId = 1;

Console.WriteLine("=== Deck Archetype Clustering (HDBSCAN) ===");
Console.WriteLine();
Console.WriteLine("Configuration:");
Console.WriteLine($"  LeaderWeightMultiplier:       {config.LeaderWeightMultiplier}");
Console.WriteLine($"  BaseWeightMultiplier:         {config.BaseWeightMultiplier}");
Console.WriteLine($"  MainDeckWeightMultiplier:     {config.MainDeckWeightMultiplier}");
Console.WriteLine($"  MaxIdfWeight:                 {config.MaxIdfWeight}");
Console.WriteLine($"  MinimumDocumentFrequency:     {config.MinimumDocumentFrequency}");
Console.WriteLine($"  MinClusterSize:               {config.MinClusterSize}");
Console.WriteLine($"  MinPoints:                    {config.MinPoints}");
Console.WriteLine($"  RequireSameLeaderForSimilarity: {config.RequireSameLeaderForSimilarity}");
Console.WriteLine($"  DifferentLeaderPenalty:       {config.DifferentLeaderPenalty}");
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

// === Exact Deck Deduplication ===
Console.WriteLine("Deduplicating decks...");
var (uniqueDecks, duplicateCounts, totalRepresented) = DeduplicateDecks(decks);
Console.WriteLine($"  Unique decks: {uniqueDecks.Count} (from {totalRepresented} total, {totalRepresented - uniqueDecks.Count} duplicates removed)");
Console.WriteLine();

// Build feature vectors with smoothed TF-IDF weighting and separate leader/base emphasis
Console.WriteLine("Building feature vectors (smoothed TF-IDF, separate leader/base weights)...");
var buildResult = BuildFeatureVectors(uniqueDecks, config);
Console.WriteLine($"  Feature vector dimension: {buildResult.CardIdIndex.Count} unique cards");
Console.WriteLine($"  Cards filtered (doc freq < {config.MinimumDocumentFrequency}): {buildResult.FilteredCardCount}");
Console.WriteLine();

// Build distance function using cosine distance (vectors are already normalized)
Func<int, int, double> distanceFunc = (indexA, indexB) =>
{
    var a = buildResult.FeatureVectors[indexA];
    var b = buildResult.FeatureVectors[indexB];
    double dotProduct = 0;

    for (int i = 0; i < a.Length; i++)
        dotProduct += a[i] * b[i];

    var similarity = Math.Max(0, Math.Min(1, dotProduct)); // Clamp for numerical stability

    // Optional: penalize different leaders
    if (config.RequireSameLeaderForSimilarity)
    {
        var leaderA = GetLeaderCardId(uniqueDecks[indexA]);
        var leaderB = GetLeaderCardId(uniqueDecks[indexB]);
        if (leaderA != leaderB)
            similarity *= (1.0 - config.DifferentLeaderPenalty);
    }

    return 1.0 - similarity;
};

// Run HDBSCAN clustering
Console.WriteLine("Running HDBSCAN clustering...");
var result = HdbscanRunner.Run(
    datasetCount: buildResult.FeatureVectors.Length,
    minPoints: config.MinPoints,
    minClusterSize: config.MinClusterSize,
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

// Calculate cohesion metrics for each cluster
var cohesionMetrics = CalculateCohesionMetrics(clusterGroups, buildResult.FeatureVectors);

Console.WriteLine();
Console.WriteLine($"{"",3}=== Results ===");
Console.WriteLine($"  Total decks fetched:    {decks.Count}");
Console.WriteLine($"  Unique decks analyzed:  {uniqueDecks.Count}");
Console.WriteLine($"  Total represented:      {totalRepresented}");
Console.WriteLine($"  Archetypes found:       {archetypeCount}");
Console.WriteLine($"  Noise/unclustered:      {noiseCount}");
Console.WriteLine();

// Interactive exploration
while (true)
{
    Console.WriteLine("--- Archetype Overview ---");
    foreach (var cluster in clusterGroups.OrderBy(c => c.Key))
    {
        var label = cluster.Key == 0 ? "Noise (unclustered)" : $"Archetype {cluster.Key}";
        var deckCount = cluster.Value.Count;
        var representedCount = cluster.Value.Sum(i => duplicateCounts[i]);
        var cohesion = cohesionMetrics.ContainsKey(cluster.Key) ? cohesionMetrics[cluster.Key] : null;

        if (cohesion != null)
            Console.WriteLine($"  [{cluster.Key}] {label}: {deckCount} decks ({representedCount} represented) | cohesion: {cohesion.Average:F2} (min: {cohesion.Min:F2}, max: {cohesion.Max:F2})");
        else
            Console.WriteLine($"  [{cluster.Key}] {label}: {deckCount} decks ({representedCount} represented)");
    }
    Console.WriteLine();
    Console.Write("Enter command - number to view archetype, 'sim A B' for similarity debug, 'q' to quit: ");
    var selection = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(selection) || selection.Equals("q", StringComparison.OrdinalIgnoreCase))
        break;

    // Similarity debugging: 'sim A B'
    if (selection.StartsWith("sim ", StringComparison.OrdinalIgnoreCase))
    {
        var parts = selection.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 && int.TryParse(parts[1], out var idxA) && int.TryParse(parts[2], out var idxB)
            && idxA >= 0 && idxA < uniqueDecks.Count && idxB >= 0 && idxB < uniqueDecks.Count)
        {
            PrintSimilarityDebug(idxA, idxB, uniqueDecks, buildResult, config);
        }
        else
        {
            Console.WriteLine("Usage: sim <deckIndexA> <deckIndexB>");
        }
        Console.WriteLine();
        continue;
    }

    if (!int.TryParse(selection, out var clusterId) || !clusterGroups.ContainsKey(clusterId))
    {
        Console.WriteLine("Invalid selection. Please try again.");
        Console.WriteLine();
        continue;
    }

    var deckIndices = clusterGroups[clusterId];
    var clusterLabel = clusterId == 0 ? "Noise (unclustered)" : $"Archetype {clusterId}";
    var clusterRepresented = deckIndices.Sum(i => duplicateCounts[i]);

    Console.WriteLine();
    Console.WriteLine($"=== {clusterLabel} ===");
    Console.WriteLine($"  Unique decks: {deckIndices.Count}");
    Console.WriteLine($"  Total represented: {clusterRepresented}");

    if (cohesionMetrics.ContainsKey(clusterId))
    {
        var c = cohesionMetrics[clusterId];
        Console.WriteLine($"  Cohesion: {c.Average:F2} (min: {c.Min:F2}, max: {c.Max:F2})");
    }

    // Find core cards - prioritize leaders/bases first, then high-frequency cards
    var coreCards = GetCoreCards(deckIndices, uniqueDecks, buildResult, config);

    if (coreCards.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("  Core cards:");
        foreach (var card in coreCards)
        {
            var percentage = card.ClusterFreq * 100;
            var tag = card.CardType switch
            {
                CardSlotType.Leader => " [LEADER]",
                CardSlotType.Base => " [BASE]",
                _ => ""
            };
            Console.WriteLine($"    Card ID {card.CardId}{tag}: in {card.Count}/{deckIndices.Count} decks ({percentage:F0}%), defining score: {card.DefiningScore:F2}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("  Decks:");
    foreach (var idx in deckIndices)
    {
        var deck = uniqueDecks[idx];
        var dupCount = duplicateCounts[idx];
        var dupLabel = dupCount > 1 ? $" (x{dupCount} copies)" : "";
        Console.WriteLine($"    [{idx}] [{deck.Id}] \"{deck.Name}\" (Cards: {deck.Cards.Length}, Likes: {deck.AmountOfLikes}){dupLabel}");
    }
    Console.WriteLine();
}

Console.WriteLine("Goodbye!");

// === Helper methods ===

static int GetLeaderCardId(DeckApiModel deck)
{
    var leader = deck.Cards.FirstOrDefault(c => c.SlotId == (int)CardSlotType.Leader);
    return leader?.CardId ?? 0;
}

static (List<DeckApiModel> uniqueDecks, int[] duplicateCounts, int totalRepresented) DeduplicateDecks(List<DeckApiModel> decks)
{
    var hashMap = new Dictionary<string, (DeckApiModel Deck, int Count)>();

    foreach (var deck in decks)
    {
        var hash = ComputeDeckHash(deck);
        if (hashMap.ContainsKey(hash))
            hashMap[hash] = (hashMap[hash].Deck, hashMap[hash].Count + 1);
        else
            hashMap[hash] = (deck, 1);
    }

    var uniqueDecks = new List<DeckApiModel>();
    var counts = new List<int>();

    foreach (var (_, (deck, count)) in hashMap)
    {
        uniqueDecks.Add(deck);
        counts.Add(count);
    }

    return (uniqueDecks, counts.ToArray(), decks.Count);
}

static string ComputeDeckHash(DeckApiModel deck)
{
    var leader = deck.Cards.Where(c => c.SlotId == (int)CardSlotType.Leader).Select(c => c.CardId).OrderBy(x => x);
    var baseCards = deck.Cards.Where(c => c.SlotId == (int)CardSlotType.Base).Select(c => c.CardId).OrderBy(x => x);
    var mainDeck = deck.Cards.Where(c => c.SlotId != (int)CardSlotType.Leader && c.SlotId != (int)CardSlotType.Base)
        .OrderBy(c => c.CardId)
        .Select(c => $"{c.CardId}:{c.Amount}");

    var canonical = string.Join("|",
        "L:" + string.Join(",", leader),
        "B:" + string.Join(",", baseCards),
        "M:" + string.Join(",", mainDeck));

    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
    return Convert.ToHexString(bytes);
}

static FeatureVectorResult BuildFeatureVectors(List<DeckApiModel> decks, ClusteringConfig config)
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

    // Build a map of cardId -> SlotId (use the first occurrence for classification)
    var cardSlotMap = new Dictionary<int, int>();
    foreach (var deck in decks)
    {
        foreach (var card in deck.Cards)
        {
            if (!cardSlotMap.ContainsKey(card.CardId))
                cardSlotMap[card.CardId] = card.SlotId;
        }
    }

    // Calculate document frequency
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

    // Count filtered cards
    int filteredCount = 0;
    for (int i = 0; i < allCardIds.Count; i++)
    {
        if (documentFrequency[i] < config.MinimumDocumentFrequency)
            filteredCount++;
    }

    // Smoothed IDF: log(1 + totalDecks / (1 + documentFrequency)), clamped
    var idfWeights = new double[allCardIds.Count];
    for (int i = 0; i < allCardIds.Count; i++)
    {
        if (documentFrequency[i] < config.MinimumDocumentFrequency)
        {
            // Ultra-rare cards get zero weight (ignored)
            idfWeights[i] = 0;
        }
        else
        {
            var rawIdf = Math.Log(1.0 + (double)totalDecks / (1.0 + documentFrequency[i]));
            idfWeights[i] = Math.Min(rawIdf, config.MaxIdfWeight);
        }
    }

    // Build feature vectors with TF-IDF weighting and separate leader/base/main multipliers
    var vectors = new double[decks.Count][];
    for (int i = 0; i < decks.Count; i++)
    {
        vectors[i] = new double[allCardIds.Count];
        foreach (var card in decks[i].Cards)
        {
            if (cardIdIndex.TryGetValue(card.CardId, out var idx))
            {
                // Skip ultra-rare cards
                if (idfWeights[idx] == 0)
                    continue;

                // TF = card amount
                double tf = card.Amount;

                // Apply TF-IDF weighting
                double tfidf = tf * idfWeights[idx];

                // Apply slot-specific weight multiplier
                var slotType = GetCardSlotType(card.SlotId);
                tfidf *= slotType switch
                {
                    CardSlotType.Leader => config.LeaderWeightMultiplier,
                    CardSlotType.Base => config.BaseWeightMultiplier,
                    _ => config.MainDeckWeightMultiplier
                };

                vectors[i][idx] = tfidf;
            }
        }

        // Normalize vector to unit length
        double magnitude = 0;
        for (int j = 0; j < vectors[i].Length; j++)
            magnitude += vectors[i][j] * vectors[i][j];

        if (magnitude > 0)
        {
            magnitude = Math.Sqrt(magnitude);
            for (int j = 0; j < vectors[i].Length; j++)
                vectors[i][j] /= magnitude;
        }
    }

    return new FeatureVectorResult(vectors, cardIdIndex, idfWeights, cardSlotMap, documentFrequency, filteredCount);
}

static CardSlotType GetCardSlotType(int slotId)
{
    return slotId switch
    {
        (int)CardSlotType.Leader => CardSlotType.Leader,
        (int)CardSlotType.Base => CardSlotType.Base,
        _ => CardSlotType.MainDeck
    };
}

static Dictionary<int, CohesionMetric> CalculateCohesionMetrics(Dictionary<int, List<int>> clusterGroups, double[][] featureVectors)
{
    var metrics = new Dictionary<int, CohesionMetric>();

    foreach (var (clusterId, indices) in clusterGroups)
    {
        if (indices.Count < 2)
        {
            metrics[clusterId] = new CohesionMetric(1.0, 1.0, 1.0);
            continue;
        }

        double totalSim = 0;
        double minSim = double.MaxValue;
        double maxSim = double.MinValue;
        int pairCount = 0;

        for (int i = 0; i < indices.Count; i++)
        {
            for (int j = i + 1; j < indices.Count; j++)
            {
                var a = featureVectors[indices[i]];
                var b = featureVectors[indices[j]];
                double dot = 0;
                for (int k = 0; k < a.Length; k++)
                    dot += a[k] * b[k];

                var sim = Math.Max(0, Math.Min(1, dot));
                totalSim += sim;
                minSim = Math.Min(minSim, sim);
                maxSim = Math.Max(maxSim, sim);
                pairCount++;
            }
        }

        var avg = pairCount > 0 ? totalSim / pairCount : 1.0;
        metrics[clusterId] = new CohesionMetric(avg, minSim == double.MaxValue ? 0 : minSim, maxSim == double.MinValue ? 0 : maxSim);
    }

    return metrics;
}

static List<CoreCardInfo> GetCoreCards(List<int> deckIndices, List<DeckApiModel> decks, FeatureVectorResult buildResult, ClusteringConfig config)
{
    var cardClusterFrequency = new Dictionary<int, int>();
    foreach (var idx in deckIndices)
    {
        foreach (var card in decks[idx].Cards)
        {
            if (!cardClusterFrequency.ContainsKey(card.CardId))
                cardClusterFrequency[card.CardId] = 0;
            cardClusterFrequency[card.CardId]++;
        }
    }

    var coreCards = cardClusterFrequency
        .Select(kv =>
        {
            var cardId = kv.Key;
            var clusterFreq = (double)kv.Value / deckIndices.Count;
            var idfWeight = buildResult.CardIdIndex.ContainsKey(cardId) ? buildResult.IdfWeights[buildResult.CardIdIndex[cardId]] : 0.0;
            var slotId = buildResult.CardSlotMap.ContainsKey(cardId) ? buildResult.CardSlotMap[cardId] : 0;
            var cardType = GetCardSlotType(slotId);

            // Reduce influence of ultra-high IDF: use sqrt dampening
            var dampenedIdf = Math.Sqrt(idfWeight);

            // Prioritize frequency more than raw IDF
            var definingScore = (clusterFreq * 2.0) * dampenedIdf;

            // Give leaders/bases a boost in core card display
            definingScore *= cardType switch
            {
                CardSlotType.Leader => 3.0,
                CardSlotType.Base => 2.0,
                _ => 1.0
            };

            return new CoreCardInfo(cardId, clusterFreq, definingScore, cardType, kv.Value);
        })
        .Where(c => c.ClusterFreq >= 0.4)
        .OrderByDescending(c => c.CardType == CardSlotType.Leader ? 1 : 0) // Leaders first
        .ThenByDescending(c => c.CardType == CardSlotType.Base ? 1 : 0)   // Then bases
        .ThenByDescending(c => c.DefiningScore)
        .Take(15)
        .ToList();

    return coreCards;
}

static void PrintSimilarityDebug(int idxA, int idxB, List<DeckApiModel> decks, FeatureVectorResult buildResult, ClusteringConfig config)
{
    var a = buildResult.FeatureVectors[idxA];
    var b = buildResult.FeatureVectors[idxB];

    double dotProduct = 0;
    for (int i = 0; i < a.Length; i++)
        dotProduct += a[i] * b[i];

    var similarity = Math.Max(0, Math.Min(1, dotProduct));

    Console.WriteLine();
    Console.WriteLine($"=== Similarity Debug: Deck [{idxA}] vs Deck [{idxB}] ===");
    Console.WriteLine($"  Deck A: [{decks[idxA].Id}] \"{decks[idxA].Name}\"");
    Console.WriteLine($"  Deck B: [{decks[idxB].Id}] \"{decks[idxB].Name}\"");
    Console.WriteLine($"  Cosine similarity: {similarity:F4}");
    Console.WriteLine($"  Distance: {1.0 - similarity:F4}");

    if (config.RequireSameLeaderForSimilarity)
    {
        var leaderA = GetLeaderCardId(decks[idxA]);
        var leaderB = GetLeaderCardId(decks[idxB]);
        if (leaderA != leaderB)
        {
            var penalizedSim = similarity * (1.0 - config.DifferentLeaderPenalty);
            Console.WriteLine($"  Different leaders ({leaderA} vs {leaderB}): penalized similarity = {penalizedSim:F4}");
        }
        else
        {
            Console.WriteLine($"  Same leader ({leaderA}): no penalty applied");
        }
    }

    // Calculate per-card contributions
    var reverseIndex = buildResult.CardIdIndex.ToDictionary(kv => kv.Value, kv => kv.Key);
    var contributions = new List<(int CardId, double Contribution, int SlotId)>();

    for (int i = 0; i < a.Length; i++)
    {
        var contribution = a[i] * b[i];
        if (contribution > 0 && reverseIndex.ContainsKey(i))
        {
            var cardId = reverseIndex[i];
            var slotId = buildResult.CardSlotMap.ContainsKey(cardId) ? buildResult.CardSlotMap[cardId] : 0;
            contributions.Add((cardId, contribution, slotId));
        }
    }

    var topContributions = contributions.OrderByDescending(c => c.Contribution).Take(10).ToList();

    if (topContributions.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine("  Top similarity contributors:");
        foreach (var (cardId, contribution, slotId) in topContributions)
        {
            var cardType = GetCardSlotType(slotId);
            var tag = cardType switch
            {
                CardSlotType.Leader => " [LEADER]",
                CardSlotType.Base => " [BASE]",
                _ => ""
            };
            Console.WriteLine($"    Card {cardId}{tag} -> {contribution:F4}");
        }
    }
}

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

// === Configuration class ===

public class ClusteringConfig
{
    // Weight multipliers for different card slot types
    public double LeaderWeightMultiplier { get; set; } = 12.0;
    public double BaseWeightMultiplier { get; set; } = 4.0;
    public double MainDeckWeightMultiplier { get; set; } = 1.0;

    // IDF configuration
    public double MaxIdfWeight { get; set; } = 3.0;
    public int MinimumDocumentFrequency { get; set; } = 3;

    // HDBSCAN parameters
    public int MinClusterSize { get; set; } = 3;
    public int MinPoints { get; set; } = 2;

    // Optional leader separation experiment
    public bool RequireSameLeaderForSimilarity { get; set; } = false;
    public double DifferentLeaderPenalty { get; set; } = 0.8; // 0.0 = no penalty, 1.0 = completely dissimilar
}

// === Data types ===

public enum CardSlotType
{
    Leader = 1,
    Base = 2,
    MainDeck = 0  // Default/fallback for all other slots
}

public record FeatureVectorResult(
    double[][] FeatureVectors,
    Dictionary<int, int> CardIdIndex,
    double[] IdfWeights,
    Dictionary<int, int> CardSlotMap,
    int[] DocumentFrequency,
    int FilteredCardCount);

public record CohesionMetric(double Average, double Min, double Max);

public record CoreCardInfo(int CardId, double ClusterFreq, double DefiningScore, CardSlotType CardType, int Count);

// === API Models ===

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
