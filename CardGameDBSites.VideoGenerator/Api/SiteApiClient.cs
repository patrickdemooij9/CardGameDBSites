using System.Net.Http.Json;
using System.Text.Json;

namespace CardGameDBSites.VideoGenerator.Api;

public class SiteApiClient(string baseUrl)
{
    // The card search treats variant type 0 as the normal (non-variant) printing.
    public const int NormalVariantTypeId = 0;

    private const int PageSize = 500;

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DeckDto> GetDeck(int id) =>
        await HttpClient.GetFromJsonAsync<DeckDto>($"{baseUrl}/api/decks/get?id={id}", JsonOptions)
        ?? throw new InvalidOperationException($"Deck {id} not found");

    public async Task<IReadOnlyList<CardDto>> GetCards(IEnumerable<int> ids)
    {
        using var response = await HttpClient.PostAsJsonAsync($"{baseUrl}/api/cards/byIds", ids.Distinct().ToArray(), JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CardDto[]>(JsonOptions) ?? [];
    }

    public async Task<IReadOnlyList<SetDto>> GetSets() =>
        await HttpClient.GetFromJsonAsync<SetDto[]>($"{baseUrl}/api/sets/getAll", JsonOptions) ?? [];

    public async Task<IReadOnlyList<VariantTypeDto>> GetVariantTypes() =>
        await HttpClient.GetFromJsonAsync<VariantTypeDto[]>($"{baseUrl}/api/cards/variantTypes", JsonOptions) ?? [];

    public async Task<IReadOnlyList<CardDto>> GetSetCards(int setId, int variantTypeId)
    {
        var cards = new List<CardDto>();
        for (var page = 1; ; page++)
        {
            var query = new { pageNumber = page, pageSize = PageSize, setId, variantTypeIds = new[] { variantTypeId } };
            using var response = await HttpClient.PostAsJsonAsync($"{baseUrl}/api/cards/query", query, JsonOptions);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardPageDto>(JsonOptions);

            cards.AddRange(result?.Items ?? []);
            if (result is null || result.Items.Count == 0 || cards.Count >= result.TotalItems) return cards;
        }
    }
}
