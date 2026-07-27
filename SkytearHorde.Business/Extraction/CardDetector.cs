using OpenCvSharp;

namespace SkytearHorde.Business.Extraction
{
    /// <summary>
    /// Deterministic card detection using OpenCV contour analysis. Inputs are clean, axis-aligned
    /// digital card renders on a plain/glow background, so axis-aligned bounding rectangles are
    /// sufficient — no perspective correction is needed. Returns one rectangle per detected card,
    /// in reading order (top-to-bottom, then left-to-right).
    /// </summary>
    internal static class CardDetector
    {
        // Tuning knobs — kept together for easy adjustment.
        private const double MinAreaFraction = 0.03;   // a card must cover at least this fraction of the image
        private const double MinFillRatio = 0.80;      // contour area / bounding-rect area — rejects thin edges / art blobs
        private const double MinAspect = 0.55;         // allow portrait (~0.71) ...
        private const double MaxAspect = 1.85;         // ... and landscape (~1.4) cards, with tolerance

        /// <summary>
        /// When set to a folder path, each detection run also writes debug images there: the edge/morphology
        /// mask and the source with all contours (yellow) and the selected card rectangles (red) drawn on it.
        /// Left null in normal operation. Set from config (CardImport:DebugDetection) by the caller.
        /// </summary>
        public static string? DebugOutputDirectory { get; set; }

        public static List<(int X, int Y, int W, int H)> DetectCards(byte[] imageBytes)
        {
            using var src = Cv2.ImDecode(imageBytes, ImreadModes.Color);
            if (src.Empty()) return [];

            double imageArea = (double)src.Width * src.Height;

            using var gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            using var blurred = new Mat();
            Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

            // Separate the light background + soft glow from the darker, coloured card via an Otsu threshold
            // (foreground/card = white). This is far more robust than edge detection here: the card's busy
            // interior art produces a mass of internal edges, but as a filled region the card is one solid blob.
            using var mask = new Mat();
            Cv2.Threshold(blurred, mask, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

            // Close small holes (light spots inside the art) so each card stays a single solid blob.
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(7, 7));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel, iterations: 2);

            Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var kept = new List<Rect>();
            foreach (var contour in contours)
            {
                var rect = Cv2.BoundingRect(contour);
                double rectArea = (double)rect.Width * rect.Height;
                if (rectArea < imageArea * MinAreaFraction) continue;

                // Must be a solid rectangle, not a thin edge or a ragged art region.
                var contourArea = Cv2.ContourArea(contour);
                if (contourArea < rectArea * MinFillRatio) continue;

                double aspect = (double)rect.Width / rect.Height;
                if (aspect < MinAspect || aspect > MaxAspect) continue;

                kept.Add(rect);
            }

            // Drop rectangles fully contained inside a larger kept one (nested contours from the frame).
            var deduped = new List<Rect>();
            for (var i = 0; i < kept.Count; i++)
            {
                var inner = false;
                for (var j = 0; j < kept.Count; j++)
                {
                    if (i == j) continue;
                    if (Contains(kept[j], kept[i]) && Area(kept[j]) > Area(kept[i]))
                    {
                        inner = true;
                        break;
                    }
                }
                if (!inner) deduped.Add(kept[i]);
            }

            if (!string.IsNullOrWhiteSpace(DebugOutputDirectory))
                SaveDebug(src, mask, contours, deduped);

            return SortReadingOrder(deduped)
                .Select(r => (r.X, r.Y, r.Width, r.Height))
                .ToList();
        }

        /// <summary>Writes the foreground mask and a contour/rectangle overlay to <see cref="DebugOutputDirectory"/>.</summary>
        private static void SaveDebug(Mat src, Mat mask, Point[][] contours, List<Rect> selected)
        {
            try
            {
                var dir = DebugOutputDirectory!;
                Directory.CreateDirectory(dir);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                var id = Guid.NewGuid().ToString("N")[..8];
                var baseName = Path.Combine(dir, $"{stamp}_{id}");

                // 1) The foreground (card silhouette) mask fed into FindContours.
                Cv2.ImWrite($"{baseName}_1_mask.png", mask);

                // 2) The source with every detected contour (yellow) and the selected card rects (red).
                using var overlay = src.Clone();
                Cv2.DrawContours(overlay, contours, -1, new Scalar(0, 255, 255), 1);
                foreach (var r in selected)
                    Cv2.Rectangle(overlay, r, new Scalar(0, 0, 255), 3);
                Cv2.ImWrite($"{baseName}_2_contours.png", overlay);
            }
            catch
            {
                // Debug output must never break detection.
            }
        }

        private static long Area(Rect r) => (long)r.Width * r.Height;

        private static bool Contains(Rect outer, Rect inner) =>
            inner.X >= outer.X && inner.Y >= outer.Y &&
            inner.X + inner.Width <= outer.X + outer.Width &&
            inner.Y + inner.Height <= outer.Y + outer.Height;

        /// <summary>Orders rectangles top-to-bottom in row bands, left-to-right within each band.</summary>
        private static List<Rect> SortReadingOrder(List<Rect> rects)
        {
            var remaining = rects.OrderBy(r => r.Y).ToList();
            var ordered = new List<Rect>();

            while (remaining.Count > 0)
            {
                var top = remaining[0];
                var bandThreshold = top.Y + top.Height / 2; // same row = starts above the top card's vertical centre
                var row = remaining.Where(r => r.Y < bandThreshold).OrderBy(r => r.X).ToList();
                ordered.AddRange(row);
                remaining.RemoveAll(r => r.Y < bandThreshold);
            }

            return ordered;
        }
    }
}
