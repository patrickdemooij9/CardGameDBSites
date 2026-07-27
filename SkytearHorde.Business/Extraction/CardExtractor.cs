using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SkytearHorde.Business.Extraction
{
    public record ExtractedCard(string Label, string Base64);

    /// <summary>
    /// Detects individual trading cards within an image (a single card or a multi-card reveal) using
    /// deterministic OpenCV contour detection, then crops and normalizes each one.
    /// </summary>
    public class CardExtractor
    {
        public const int TargetWidth = 595;
        public const int TargetHeight = 828;

        // The detection prompt is no longer used (detection is deterministic); the parameter is kept
        // so existing call sites don't need to change.
        public CardExtractor(string detectionPrompt)
        {
        }

        /// <param name="resize">
        /// When true (default) each detected card is padded/resized to the standard <see cref="TargetWidth"/>x
        /// <see cref="TargetHeight"/>. Set false to keep the cropped card at its natural size — used for
        /// back sides whose dimensions differ from the front.
        /// </param>
        public Task<List<ExtractedCard>> ExtractAsync(string apiKey, string imageBase64, string mimeType = "image/png", bool resize = true)
        {
            var imageBytes = Convert.FromBase64String(imageBase64);
            var rects = CardDetector.DetectCards(imageBytes);
            return Task.FromResult(CropAndNormalize(imageBytes, rects, resize));
        }

        private static List<ExtractedCard> CropAndNormalize(byte[] imageBytes, List<(int X, int Y, int W, int H)> rects, bool resize)
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);

            // Fallback: if detection found nothing, treat the whole image as one card rather than dropping it.
            if (rects.Count == 0)
                rects = [(0, 0, image.Width, image.Height)];

            var results = new List<ExtractedCard>();
            foreach (var (rx, ry, rw, rh) in rects)
            {
                var x = Math.Clamp(rx, 0, image.Width - 1);
                var y = Math.Clamp(ry, 0, image.Height - 1);
                var w = Math.Clamp(rw, 1, image.Width - x);
                var h = Math.Clamp(rh, 1, image.Height - y);

                using var cropped = image.Clone(ctx =>
                {
                    ctx.Crop(new Rectangle(x, y, w, h));
                    // Scale to fit within the target box preserving aspect ratio — no padding, so the
                    // output never gets letterbox/whitespace bands. Backs (resize: false) keep natural size.
                    if (resize)
                        ctx.Resize(new ResizeOptions
                        {
                            Size = new Size(TargetWidth, TargetHeight),
                            Mode = ResizeMode.Max
                        });
                });

                using var stream = new MemoryStream();
                cropped.SaveAsPng(stream);
                results.Add(new ExtractedCard("card", Convert.ToBase64String(stream.ToArray())));
            }

            return results;
        }
    }
}
