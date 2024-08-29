using SkytearHorde.Business.BackgroundRunners;
using SkytearHorde.Business.Config;
using System.Net.Http.Json;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerAccessTokenGetter
    {
        public async Task<string> Get(CardGameSettingsConfig config, HttpClient httpClient)
        {
            return (await(await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.tcgplayer.com/token")
            {
                Content = new FormUrlEncodedContent(
                [
                    new("grant_type", "client_credentials"),
                    new("client_id", config.TcgPlayerApiKey),
                    new("client_secret", config.TcgPlayerApiSecret)
                ])
            })).Content.ReadFromJsonAsync<TcgPlayerAccessToken>())!.AccessToken;
        }
    }
}
