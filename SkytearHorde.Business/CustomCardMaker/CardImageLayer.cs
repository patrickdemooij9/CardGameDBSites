using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace SkytearHorde.Business.CustomCardMaker
{
    public class CardImageLayer : ICardLayer
    {
        private readonly string _imagePath;
        private readonly Point _location;
        private readonly Size? _resize;
        private readonly HttpClient _httpClient;

        private string[] _allowedExtensions = [".png", ".jpg", ".jpeg", ".webp"];
        private string[] _allowedContentType = ["image/png", "image/jpg", "image/jpeg", "image/webp"];
        private string[] _allowedHostnamesToSkipExtension = ["drive.usercontent.google.com"];

        public CardImageLayer(string imagePath, Point location, Size? resize, HttpClient httpClient)
        {
            _imagePath = imagePath;
            _location = location;
            _resize = resize;

            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task Render(Image image)
        {
            try
            {
                if (Uri.TryCreate(_imagePath, UriKind.Absolute, out var uri) && uri.Scheme == "https" && (_allowedExtensions.Any(uri.AbsolutePath.EndsWith) || _allowedHostnamesToSkipExtension.Contains(uri.Host)))
                {
                    var request = await _httpClient.GetAsync(_imagePath);
                    if (!request.IsSuccessStatusCode) return;
                    if (!_allowedContentType.Contains(request.Content.Headers.ContentType?.MediaType)) return;

                    using var bytes = await request.Content.ReadAsStreamAsync();
                    var loadedImage = await Image.LoadAsync(new DecoderOptions()
                    {
                        SkipMetadata = true,
                    }, bytes);

                    if (_resize.HasValue)
                    {
                        loadedImage.Mutate(it => it.Resize(_resize.Value));
                    }

                    image.Mutate(it => it.DrawImage(loadedImage, _location, 1));
                }
                else
                {
                    using var loadedImage = await Image.LoadAsync(_imagePath);

                    if (_resize.HasValue)
                    {
                        loadedImage.Mutate(it => it.Resize(_resize.Value));
                    }

                    image.Mutate(it => it.DrawImage(loadedImage, _location, 1));
                }
            }
            catch
            {

            }
        }
    }
}
