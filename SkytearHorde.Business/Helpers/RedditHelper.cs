using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkytearHorde.Business.Helpers
{
    public class RedditApiClient
    {
        private const string TokenUrl = "https://www.reddit.com/api/v1/access_token";
        private const string SubmitUrl = "https://oauth.reddit.com/api/submit";
        private const string CommentUrl = "https://oauth.reddit.com/api/comment";
        private const string UserAgent = "CardGameDBSites/1.0 (by /u/Patrickdemooij9)";

        private readonly HttpClient _httpClient;
        private readonly string _username;
        private readonly string _password;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public RedditApiClient(HttpClient httpClient, string username, string password, string clientId, string clientSecret)
        {
            _httpClient = httpClient;
            _username = username;
            _password = password;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", _username),
                new KeyValuePair<string, string>("password", _password),
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

        public async Task SubmitPostAsync(string subreddit, string title, string text, string flairId)
        {
            var token = await GetAccessTokenAsync();

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

        public async Task<List<RedditComment>> GetNewCommentsAsync(string subreddit, int limit = 25)
        {
            var token = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/comments.json?limit={limit}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit get comments request failed with status {response.StatusCode}: {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var comments = new List<RedditComment>();

            var children = doc.RootElement.GetProperty("data").GetProperty("children");
            foreach (var child in children.EnumerateArray())
            {
                var data = child.GetProperty("data");
                var fullName = data.GetProperty("name").GetString();
                var body = data.GetProperty("body").GetString();
                var author = data.GetProperty("author").GetString();
                var createdUtc = data.GetProperty("created_utc").GetDouble();
                if (fullName != null && body != null)
                {
                    comments.Add(new RedditComment
                    {
                        FullName = fullName,
                        Body = body,
                        Author = author ?? string.Empty,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)createdUtc).UtcDateTime
                    });
                }
            }

            return comments;
        }

        public async Task<List<RedditPost>> GetNewPostsAsync(string subreddit, int limit = 25)
        {
            var token = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/new.json?limit={limit}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit get posts request failed with status {response.StatusCode}: {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var posts = new List<RedditPost>();

            var children = doc.RootElement.GetProperty("data").GetProperty("children");
            foreach (var child in children.EnumerateArray())
            {
                var data = child.GetProperty("data");
                var fullName = data.GetProperty("name").GetString();
                var title = data.GetProperty("title").GetString();
                var selfText = data.GetProperty("selftext").GetString();
                var author = data.GetProperty("author").GetString();
                var createdUtc = data.GetProperty("created_utc").GetDouble();
                if (fullName != null)
                {
                    posts.Add(new RedditPost
                    {
                        FullName = fullName,
                        Title = title ?? string.Empty,
                        SelfText = selfText ?? string.Empty,
                        Author = author ?? string.Empty,
                        CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)createdUtc).UtcDateTime
                    });
                }
            }

            return posts;
        }

        public async Task ReplyToCommentAsync(string commentFullName, string text)
        {
            var token = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, CommentUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("parent", commentFullName),
                new KeyValuePair<string, string>("text", text),
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit reply request failed with status {response.StatusCode}: {body}");
            }
        }
    }

    public class RedditComment
    {
        public string FullName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RedditPost
    {
        public string FullName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SelfText { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
