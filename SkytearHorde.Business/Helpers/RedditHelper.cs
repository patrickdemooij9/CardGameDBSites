using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkytearHorde.Business.Helpers
{
    public class RedditApiClient
    {
        private const string TokenUrl = "https://www.reddit.com/api/v1/access_token";
        private const string SubmitUrl = "https://oauth.reddit.com/api/submit";
        private const string UserAgent = "CardGameDBSites/1.0 (by /u/Patrickdemooij9)";

        private readonly HttpClient _httpClient;

        public RedditApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<string> GetAccessTokenAsync(string username, string password, string clientId, string clientSecret)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit token request failed with status {response.StatusCode}: {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("No access_token in Reddit OAuth response.");
        }

        public async Task SubmitPostAsync(string username, string password, string clientId, string clientSecret,
            string subreddit, string title, string text, string flairId)
        {
            var token = await GetAccessTokenAsync(username, password, clientId, clientSecret);

            var request = new HttpRequestMessage(HttpMethod.Post, SubmitUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("kind", "self"),
                new KeyValuePair<string, string>("sr", subreddit),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("flair_id", flairId),
                new KeyValuePair<string, string>("resubmit", "true"),
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit submit request failed with status {response.StatusCode}: {body}");
            }
        }
    }
}
