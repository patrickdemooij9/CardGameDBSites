using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SkytearHorde.Business.Detection
{
    /// <summary>
    /// Detects Star Wars Unlimited aspect icons by analyzing the dominant HSL color
    /// in each icon slot of a cropped aspect strip.
    ///
    /// The aspect strip is divided into icon-sized vertical slots (icons are roughly
    /// square, so slot height ≈ strip width). Each slot's dominant color is mapped
    /// to an aspect name. Slots with no matching color are treated as empty.
    ///
    /// Color thresholds may need tuning depending on image quality and card variants.
    /// </summary>
    public class ColorAspectDetector : IPropertyDetector
    {
        private record AspectColor(string Aspect, Func<float, float, float, bool> Matcher);

        // HSL matchers per aspect. Arguments: hue (0-360), saturation (0-1), lightness (0-1).
        private static readonly List<AspectColor> AspectMatchers =
        [
            new("Aggression", (h, s, l) => (h < 25 || h > 340) && s > 0.5f && l is > 0.2f and < 0.85f),
            new("Command",    (h, s, l) => h is > 95 and < 165   && s > 0.4f && l is > 0.2f and < 0.85f),
            new("Cunning",    (h, s, l) => h is > 35 and < 75    && s > 0.4f && l is > 0.2f and < 0.85f),
            new("Vigilance",  (h, s, l) => h is > 190 and < 250  && s > 0.35f && l is > 0.2f and < 0.85f),
            new("Villainy",   (h, s, l) => l < 0.22f),
            new("Heroism",    (h, s, l) => l > 0.78f && s < 0.25f),
        ];

        private const float DetectionThreshold = 0.15f;

        public string? Detect(string base64CroppedImage)
        {
            var imageBytes = Convert.FromBase64String(base64CroppedImage);
            using var image = Image.Load<Rgba32>(imageBytes);

            int slotHeight = Math.Max(image.Width, 20);
            int numSlots = Math.Clamp((int)Math.Ceiling((double)image.Height / slotHeight), 1, 4);

            var detected = new List<string>();

            for (int slot = 0; slot < numSlots; slot++)
            {
                int slotY = slot * slotHeight;
                int slotH = Math.Min(slotHeight, image.Height - slotY);

                int sampleX = image.Width / 4;
                int sampleW = image.Width / 2;
                int sampleY = slotY + slotH / 4;
                int sampleH = slotH / 2;

                var aspect = ClassifySlot(image, sampleX, sampleY, sampleW, sampleH);
                if (aspect != null && !detected.Contains(aspect))
                    detected.Add(aspect);
            }

            return detected.Count > 0 ? string.Join(",", detected) : null;
        }

        private static string? ClassifySlot(Image<Rgba32> image, int x, int y, int width, int height)
        {
            var matchCounts = new Dictionary<string, int>();
            int totalSampled = 0;

            for (int py = y; py < y + height && py < image.Height; py++)
            {
                for (int px = x; px < x + width && px < image.Width; px++)
                {
                    var pixel = image[px, py];
                    if (pixel.A < 128) continue;

                    var (h, s, l) = RgbToHsl(pixel.R, pixel.G, pixel.B);
                    totalSampled++;

                    foreach (var matcher in AspectMatchers)
                    {
                        if (matcher.Matcher(h, s, l))
                        {
                            matchCounts.TryAdd(matcher.Aspect, 0);
                            matchCounts[matcher.Aspect]++;
                            break;
                        }
                    }
                }
            }

            if (totalSampled == 0 || matchCounts.Count == 0) return null;

            var best = matchCounts.MaxBy(kv => kv.Value);
            return (float)best.Value / totalSampled >= DetectionThreshold ? best.Key : null;
        }

        private static (float h, float s, float l) RgbToHsl(byte r, byte g, byte b)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = MathF.Max(rf, MathF.Max(gf, bf));
            float min = MathF.Min(rf, MathF.Min(gf, bf));
            float l = (max + min) / 2f;

            if (max == min) return (0f, 0f, l);

            float d = max - min;
            float s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            float h;
            if (max == rf)      h = ((gf - bf) / d + (gf < bf ? 6f : 0f)) / 6f;
            else if (max == gf) h = ((bf - rf) / d + 2f) / 6f;
            else                h = ((rf - gf) / d + 4f) / 6f;

            return (h * 360f, s, l);
        }
    }
}
