using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using Path = System.IO.Path;
using Image = SixLabors.ImageSharp.Image;
using File = System.IO.File;

namespace SkytearHorde.Business.Exports
{
    /// <summary>
    /// Shared drawing primitives for the 1080x1920 dark/dramatic infographic carousels
    /// (background, logo, footer, headers, CTA, bars, card images, text helpers).
    /// Concrete renderers (tournament, facts) subclass this and add slide methods.
    /// </summary>
    public abstract class InfographicRendererBase
    {
        protected const int Width = 1080;
        protected const int Height = 1920;
        protected const int Margin = 80;

        // Dark & dramatic palette
        protected static readonly Color BgTop = Color.ParseHex("#0B1020");
        protected static readonly Color BgBottom = Color.ParseHex("#1A2244");
        protected static readonly Color Accent = Color.ParseHex("#FFC94A");
        protected static readonly Color TextColor = Color.White;
        protected static readonly Color MutedColor = Color.ParseHex("#9AA3C0");
        protected static readonly Color Ink = Color.ParseHex("#20263F");

        protected static readonly Color Gold = Color.ParseHex("#FFCF5A");
        protected static readonly Color Silver = Color.ParseHex("#CBD5E1");
        protected static readonly Color Bronze = Color.ParseHex("#D08A4E");
        protected static readonly Color ChipColor = Color.ParseHex("#2A3358");
        protected static readonly Color TrackColor = Color.ParseHex("#232B4A");

        private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        private readonly IWebHostEnvironment _webHostEnvironment;

        protected readonly FontFamily ExtraBold;
        protected readonly FontFamily Bold;
        protected readonly FontFamily SemiBold;
        protected readonly FontFamily Medium;

        protected InfographicRendererBase(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;

            ExtraBold = LoadFont("Barlow-ExtraBold.ttf");
            Bold = LoadFont("Barlow-Bold.ttf");
            SemiBold = LoadFont("Barlow-SemiBold.ttf");
            Medium = LoadFont("Barlow-Medium.ttf");
        }

        private FontFamily LoadFont(string fileName)
        {
            var collection = new FontCollection();
            return collection.Add(Path.Combine(_webHostEnvironment.WebRootPath, "fonts", fileName));
        }

        // ---- Canvas / layout ---------------------------------------------

        protected static Image<Rgba32> NewCanvas() => new(Width, Height);

        protected void RenderBackground(Image<Rgba32> image)
        {
            image.Mutate(ctx => ctx.Fill(new LinearGradientBrush(
                new PointF(0, 0), new PointF(0, Height), GradientRepetitionMode.None,
                new ColorStop(0f, BgTop), new ColorStop(1f, BgBottom))));

            // Soft accent glow near the top for drama
            using var glow = new Image<Rgba32>(Width, Height);
            glow.Mutate(ctx => ctx.Fill(Accent.WithAlpha(0.22f), new EllipsePolygon(Width / 2f, 260, 620, 360)));
            glow.Mutate(ctx => ctx.GaussianBlur(120));
            image.Mutate(ctx => ctx.DrawImage(glow, 1));
        }

        protected async Task RenderLogo(Image<Rgba32> image, int topY)
        {
            var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "SkytearHordeLogo.png");
            if (!File.Exists(logoPath)) return;

            using var logo = await Image.LoadAsync(logoPath);
            var targetWidth = 340;
            var targetHeight = (int)(logo.Height * (targetWidth / (float)logo.Width));
            logo.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
            image.Mutate(ctx => ctx.DrawImage(logo, new Point((Width - targetWidth) / 2, topY), 1));
        }

        /// <summary>Title (top), accent underline, and a muted subtitle — the header used on data slides.</summary>
        protected void RenderHeader(Image<Rgba32> image, string title, string? subtitle, float titleSize = 74f)
        {
            DrawCenteredText(image, title, ExtraBold.CreateFont(titleSize), TextColor, 130);
            image.Mutate(ctx => ctx.Fill(Accent, new RectangularPolygon(Width / 2f - 90, 250, 180, 6)));
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                DrawCenteredText(image, subtitle!, Medium.CreateFont(40), MutedColor, 285);
            }
        }

        protected void RenderCtaPill(Image<Rgba32> image, string text, int centerY)
        {
            var font = Bold.CreateFont(48);
            var textSize = TextMeasurer.MeasureSize(text, new TextOptions(font));

            var arrowSize = 28f;
            var arrowGap = 24f;
            var padX = 60f;
            var padY = 28f;

            var contentW = textSize.Width + arrowGap + arrowSize;
            var pillW = contentW + padX * 2;
            var pillH = textSize.Height + padY * 2;
            var pillX = Width / 2f - pillW / 2f;
            var centerYf = centerY;
            var radius = pillH / 2f;

            // Pill = center rectangle + two end circles
            image.Mutate(ctx => ctx.Fill(Accent, new RectangularPolygon(pillX + radius, centerYf - pillH / 2f, pillW - radius * 2, pillH)));
            image.Mutate(ctx => ctx.Fill(Accent, new EllipsePolygon(pillX + radius, centerYf, radius)));
            image.Mutate(ctx => ctx.Fill(Accent, new EllipsePolygon(pillX + pillW - radius, centerYf, radius)));

            // Left-aligned text, then a triangle arrow, together centered in the pill
            var contentX = Width / 2f - contentW / 2f;
            var options = new RichTextOptions(font)
            {
                Origin = new Vector2(contentX, centerYf),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            image.Mutate(ctx => ctx.DrawText(options, text, Ink));

            var ax = contentX + textSize.Width + arrowGap;
            var arrow = new Polygon(new LinearLineSegment(
                new PointF(ax, centerYf - arrowSize / 2f),
                new PointF(ax + arrowSize, centerYf),
                new PointF(ax, centerYf + arrowSize / 2f)));
            image.Mutate(ctx => ctx.Fill(Ink, arrow));
        }

        protected void RenderFooter(Image<Rgba32> image, string? footerText)
        {
            if (string.IsNullOrWhiteSpace(footerText)) return;

            DrawCenteredText(image, footerText!, SemiBold.CreateFont(34), MutedColor, Height - 90, letterSpacing: 4f);
        }

        /// <summary>A rounded-cap horizontal bar (rectangle + two end circles).</summary>
        protected static void RenderRoundedBar(Image<Rgba32> image, float x, float y, float width, float height, Color color)
        {
            var radius = height / 2f;
            var w = Math.Max(width, height); // keep at least a rounded dot for tiny values
            image.Mutate(ctx => ctx.Fill(color, new EllipsePolygon(x + radius, y + radius, radius)));
            image.Mutate(ctx => ctx.Fill(color, new EllipsePolygon(x + w - radius, y + radius, radius)));
            image.Mutate(ctx => ctx.Fill(color, new RectangularPolygon(x + radius, y, w - radius * 2, height)));
        }

        // ---- Text --------------------------------------------------------

        protected void DrawCenteredText(Image<Rgba32> image, string text, Font font, Color color, float y, float letterSpacing = 0f)
        {
            var options = new RichTextOptions(font)
            {
                Origin = new Vector2(Width / 2f, y),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            if (letterSpacing > 0f)
            {
                // Emulate letter spacing by inserting spaces between characters
                text = string.Join(' ', text.ToCharArray());
            }
            image.Mutate(ctx => ctx.DrawText(options, text, color));
        }

        protected Font FitFont(string text, FontFamily family, float maxWidth, float maxHeight, float startSize, float minSize)
        {
            for (var size = startSize; size > minSize; size -= 4f)
            {
                var font = family.CreateFont(size);
                var options = new TextOptions(font)
                {
                    WrappingLength = maxWidth,
                    LineSpacing = 1.05f
                };
                var measured = TextMeasurer.MeasureSize(text, options);
                if (measured.Width <= maxWidth && measured.Height <= maxHeight)
                {
                    return font;
                }
            }
            return family.CreateFont(minSize);
        }

        // ---- Card / media images -----------------------------------------

        /// <summary>
        /// Draws a card image into <paramref name="size"/> at <paramref name="point"/>.
        /// crop=true fills the box (cropping, anchored top) for thumbnails; crop=false fits the
        /// whole card inside the box (centered) for hero shots. Falls back to a placeholder chip.
        /// </summary>
        protected async Task RenderCardImage(Image<Rgba32> image, string? imageUrl, Point point, Size size, bool crop)
        {
            try
            {
                using var source = await LoadImageAsync(imageUrl);
                if (source is null)
                {
                    RenderPlaceholder(image, point, size);
                    return;
                }

                source.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = size,
                    Mode = crop ? ResizeMode.Crop : ResizeMode.Max,
                    Position = AnchorPositionMode.Top
                }));
                ApplyRoundedCorners(source, 16f);

                // Center within the box (matters for Max-fit hero cards).
                var drawX = point.X + (size.Width - source.Width) / 2;
                var drawY = crop ? point.Y : point.Y + (size.Height - source.Height) / 2;
                image.Mutate(ctx => ctx.DrawImage(source, new Point(drawX, drawY), 1));
            }
            catch
            {
                RenderPlaceholder(image, point, size);
            }
        }

        private async Task<Image?> LoadImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return null;

            // Fast path: media present on local disk (production web server)
            if (imageUrl.StartsWith('/'))
            {
                var localPath = $"{_webHostEnvironment.WebRootPath}{imageUrl.Replace('/', Path.DirectorySeparatorChar)}";
                if (File.Exists(localPath)) return await Image.LoadAsync(localPath);
            }
            else
            {
                var relative = new Uri(imageUrl).AbsolutePath;
                var localPath = $"{_webHostEnvironment.WebRootPath}{relative.Replace('/', Path.DirectorySeparatorChar)}";
                if (File.Exists(localPath)) return await Image.LoadAsync(localPath);
            }

            // Otherwise fetch from the media host over HTTP
            if (!imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return null;

            var bytes = await HttpClient.GetByteArrayAsync(imageUrl);
            return Image.Load(bytes);
        }

        protected static void RenderPlaceholder(Image<Rgba32> image, Point point, Size size)
        {
            image.Mutate(ctx => ctx.Fill(ChipColor, new RectangularPolygon(point.X, point.Y, size.Width, size.Height)));
        }

        protected static void ApplyRoundedCorners(Image image, float cornerRadius)
        {
            var corners = BuildCorners(image.Width, image.Height, cornerRadius);
            image.Mutate(ctx =>
            {
                ctx.SetGraphicsOptions(new GraphicsOptions
                {
                    Antialias = true,
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                });
                foreach (var corner in corners)
                {
                    ctx.Fill(Color.Red, corner);
                }
            });
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        protected static async Task<byte[]> ToPng(Image image)
        {
            using var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            return stream.ToArray();
        }
    }
}
