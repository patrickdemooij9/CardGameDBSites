using System.Net.Http.Json;
using System.Text.Json;

namespace CardGameDBSites.VideoGenerator.Api;

public class SiteApiClient(string baseUrl)
{
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
}
