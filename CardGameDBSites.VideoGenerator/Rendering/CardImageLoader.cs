using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CardGameDBSites.VideoGenerator.Rendering;

public sealed class CardImageLoader : IDisposable
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(20) };

    private readonly Dictionary<string, Image<Rgba32>?> _cache = [];

    /// <summary>Returns a copy the caller owns, or null when the image can't be loaded.</summary>
    public async Task<Image<Rgba32>?> Load(string imageUrl)
    {
        if (!_cache.TryGetValue(imageUrl, out var image))
        {
            image = await Fetch(imageUrl);
            _cache[imageUrl] = image;
        }
        return image?.Clone();
    }

    private static async Task<Image<Rgba32>?> Fetch(string imageUrl)
    {
        try
        {
            if (File.Exists(imageUrl)) return await Image.LoadAsync<Rgba32>(imageUrl);

            var bytes = await HttpClient.GetByteArrayAsync(imageUrl);
            return Image.Load<Rgba32>(bytes);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not load {imageUrl}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        foreach (var image in _cache.Values) image?.Dispose();
    }
}
