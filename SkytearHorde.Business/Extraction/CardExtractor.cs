using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Extraction
{
    public record ExtractedCard(string Label, string Base64);

    /// <summary>
    /// Detects individual trading cards within a reveal image (e.g. a spoiler banner
    /// with multiple cards), then crops and normalizes each one to a standard size.
    /// </summary>
    public class CardExtractor
    {
        public const int TargetWidth = 595;
        public const int TargetHeight = 828;

        // Percentage to grow the detected bounding box on each side, giving the card breathing room.
        private const double CropMarginPercent = 4.0;

        private readonly string _detectionPrompt;

        public CardExtractor(string detectionPrompt)
        {
            _detectionPrompt = detectionPrompt;
        }

        public async Task<List<ExtractedCard>> ExtractAsync(string apiKey, string imageBase64, string mimeType = "image/png")
        {
            var detections = await DetectBoundingBoxesAsync(apiKey, imageBase64, mimeType);
            if (detections.Count == 0) return [];

            return CropAndNormalize(imageBase64, detections);
        }

        private async Task<List<DetectedRegion>> DetectBoundingBoxesAsync(string apiKey, string imageBase64, string mimeType)
        {
            var googleAI = new GoogleAI(apiKey: apiKey);
            var model = googleAI.GenerativeModel(model: Model.GeminiFlashLiteLatest);

            var request = new GenerateContentRequest(_detectionPrompt);
            request.GenerationConfig = new GenerationConfig { ResponseMimeType = "application/json" };
            request.AddPart(new InlineData { Data = imageBase64, MimeType = mimeType });

            var response = await model.GenerateContent(request);
            if (string.IsNullOrWhiteSpace(response?.Text)) return [];

            try
            {
                return JsonSerializer.Deserialize<List<DetectedRegion>>(response.Text) ?? [];
            }
            catch
            {
                return [];
            }
        }

        private static List<ExtractedCard> CropAndNormalize(string imageBase64, List<DetectedRegion> detections)
        {
            var imageBytes = Convert.FromBase64String(imageBase64);
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);

            var results = new List<ExtractedCard>();
            foreach (var detection in detections)
            {
                // Gemini returns [ymin, xmin, ymax, xmax] on a 0-1000 scale
                int x = (int)(image.Width  * detection.Box[1] / 1000.0);
                int y = (int)(image.Height * detection.Box[0] / 1000.0);
                int w = (int)(image.Width  * (detection.Box[3] - detection.Box[1]) / 1000.0);
                int h = (int)(image.Height * (detection.Box[2] - detection.Box[0]) / 1000.0);

                // Expand the detected box by a small margin so tight detections don't clip the card edges.
                int marginX = (int)(w * CropMarginPercent / 100.0);
                int marginY = (int)(h * CropMarginPercent / 100.0);
                x -= marginX;
                y -= marginY;
                w += marginX * 2;
                h += marginY * 2;

                x = Math.Clamp(x, 0, image.Width - 1);
                y = Math.Clamp(y, 0, image.Height - 1);
                w = Math.Clamp(w, 1, image.Width - x);
                h = Math.Clamp(h, 1, image.Height - y);

                // Pad (letterbox) instead of the default Crop mode so the whole card always fits
                // the target size without distortion; the leftover space is filled with black.
                var cropped = image.Clone(ctx => ctx
                    .Crop(new Rectangle(x, y, w, h))
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(TargetWidth, TargetHeight),
                        Mode = ResizeMode.Pad,
                        PadColor = Color.Black
                    }));

                using var stream = new MemoryStream();
                cropped.SaveAsPng(stream);
                results.Add(new ExtractedCard(detection.Label, Convert.ToBase64String(stream.ToArray())));
            }

            return results;
        }

        private record DetectedRegion(
            [property: JsonPropertyName("label")] string Label,
            [property: JsonPropertyName("box")]   int[]  Box
        );
    }
}
