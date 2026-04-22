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
        private const string MediaAssetUrl = "https://oauth.reddit.com/api/media/asset.json?raw_json=1";
        private const string ConvertRteBodyFormatUrl = "https://oauth.reddit.com/api/convert_rte_body_format?raw_json=1";
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

        public async Task SubmitRichTextPostWithImageAsync(string subreddit, string title, string textBelowImage, string flairId, string imageUrl)
        {
            var token = await GetAccessTokenAsync();
            var imageData = await DownloadImageAsync(imageUrl);
            var imageAsset = await CreateImageAssetAsync(token, imageData.FileName, imageData.MimeType, imageData.Bytes);

            var markdownText = $"![img]({imageAsset.AssetId} \"{title.Replace("\"", "\\\"")}\")\n\n{textBelowImage}";
            var richTextJson = await ConvertMarkdownToRtJsonAsync(token, markdownText);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{SubmitUrl}/?raw_json=1");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("kind", "self"),
                new KeyValuePair<string, string>("nsfw", "false"),
                new KeyValuePair<string, string>("resubmit", "true"),
                new KeyValuePair<string, string>("richtext_json", richTextJson),
                new KeyValuePair<string, string>("sendreplies", "true"),
                new KeyValuePair<string, string>("spoiler", "false"),
                new KeyValuePair<string, string>("sr", subreddit),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("validate_on_submit", "false"),
                new KeyValuePair<string, string>("flair_id", flairId),
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit richtext submit request failed with status {response.StatusCode}: {body}");
            }
        }

        private async Task<(string FileName, string MimeType, byte[] Bytes)> DownloadImageAsync(string imageUrl)
        {
            var response = await _httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit image download failed with status {response.StatusCode}: {body}");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0)
            {
                throw new InvalidOperationException("Reddit image download returned an empty file.");
            }

            var mimeType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            var fileName = mimeType switch
            {
                "image/png" => "card.png",
                "image/gif" => "card.gif",
                "image/webp" => "card.webp",
                _ => "card.jpg"
            };

            return (fileName, mimeType, bytes);
        }

        private async Task<RedditMediaAsset> CreateImageAssetAsync(string token, string fileName, string mimeType, byte[] bytes)
        {
            var registerRequest = new HttpRequestMessage(HttpMethod.Post, MediaAssetUrl);
            registerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            registerRequest.Headers.UserAgent.ParseAdd(UserAgent);
            registerRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("filepath", fileName),
                new KeyValuePair<string, string>("mimetype", mimeType),
            });

            var registerResponse = await _httpClient.SendAsync(registerRequest);
            if (!registerResponse.IsSuccessStatusCode)
            {
                var body = await registerResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit media asset request failed with status {registerResponse.StatusCode}: {body}");
            }

            var registerJson = await registerResponse.Content.ReadAsStringAsync();
            var registerDoc = JsonDocument.Parse(registerJson);

            var action = registerDoc.RootElement.GetProperty("args").GetProperty("action").GetString()
                ?? throw new InvalidOperationException("No Reddit media upload action provided.");
            var actionUrl = action.StartsWith("//", StringComparison.Ordinal) ? $"https:{action}" : action;
            var assetId = registerDoc.RootElement.GetProperty("asset").GetProperty("asset_id").GetString()
                ?? throw new InvalidOperationException("No Reddit media asset id provided.");

            using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, actionUrl);
            using var uploadContent = new MultipartFormDataContent();

            var fields = registerDoc.RootElement.GetProperty("args").GetProperty("fields");
            foreach (var field in fields.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString();
                var fieldValue = field.GetProperty("value").GetString();
                if (!string.IsNullOrWhiteSpace(fieldName) && fieldValue is not null)
                {
                    uploadContent.Add(new StringContent(fieldValue), fieldName);
                }
            }

            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            uploadContent.Add(fileContent, "file", fileName);
            uploadRequest.Content = uploadContent;

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                var body = await uploadResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit media upload failed with status {uploadResponse.StatusCode}: {body}");
            }

            return new RedditMediaAsset(assetId);
        }

        private async Task<string> ConvertMarkdownToRtJsonAsync(string token, string markdownText)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ConvertRteBodyFormatUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("api_type", "json"),
                new KeyValuePair<string, string>("markdown_text", markdownText),
                new KeyValuePair<string, string>("output_mode", "rtjson"),
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Reddit rtjson conversion failed with status {response.StatusCode}: {body}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var output = doc.RootElement.GetProperty("output");
            return output.GetRawText();
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
                var isSelf = data.GetProperty("is_self").GetBoolean();
                var author = data.GetProperty("author").GetString();
                var createdUtc = data.GetProperty("created_utc").GetDouble();
                if (fullName != null)
                {
                    posts.Add(new RedditPost
                    {
                        FullName = fullName,
                        Title = title ?? string.Empty,
                        SelfText = selfText ?? string.Empty,
                        IsSelf = isSelf,
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

    public record RedditMediaAsset(string AssetId);

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
        public bool IsSelf { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
