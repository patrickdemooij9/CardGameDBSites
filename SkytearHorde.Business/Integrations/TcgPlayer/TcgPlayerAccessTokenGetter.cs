using SkytearHorde.Business.BackgroundRunners;
using System.Net.Http.Json;

namespace SkytearHorde.Business.Integrations.TcgPlayer
{
    public class TcgPlayerAccessTokenGetter
    {
        public async Task<string> Get(HttpClient httpClient)
        {
            return (await(await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.tcgplayer.com/token")
            {
                Content = new FormUrlEncodedContent(
                [
                    new("grant_type", "client_credentials"),
                    new("client_id", ""),
                    new("client_secret", "")
                ])
            })).Content.ReadFromJsonAsync<TcgPlayerAccessToken>())!.AccessToken;
        }
    }
}
