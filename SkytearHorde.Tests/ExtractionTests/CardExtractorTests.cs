using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Extraction;

namespace SkytearHorde.Tests.ExtractionTests
{
    [TestFixture]
    public class CardExtractorTests
    {
        [Test]
        public async Task ExtractAsync_FullFrameCardWithLightTop_KeepsWholeCard()
        {
            var base64 = Render(715, 1000, img => img
                .BackgroundColor(Color.FromRgb(245, 235, 205))
                .Fill(Color.FromRgb(40, 60, 110), new RectangleF(30, 420, 655, 450)));

            var cards = await new CardExtractor(string.Empty).ExtractAsync(string.Empty, base64);

            Assert.That(cards, Has.Count.EqualTo(1));
            var size = SizeOf(cards[0].Base64);
            Assert.That((double)size.Width / size.Height, Is.EqualTo(715.0 / 1000).Within(0.01));
        }

        [Test]
        public async Task ExtractAsync_RevealWithTwoCards_DetectsBoth()
        {
            var base64 = Render(1600, 1000, img => img
                .BackgroundColor(Color.White)
                .Fill(Color.FromRgb(40, 60, 110), new RectangleF(150, 150, 500, 700))
                .Fill(Color.FromRgb(110, 40, 40), new RectangleF(950, 150, 500, 700)));

            var cards = await new CardExtractor(string.Empty).ExtractAsync(string.Empty, base64);

            Assert.That(cards, Has.Count.EqualTo(2));
        }

        private static string Render(int width, int height, Action<IImageProcessingContext> draw)
        {
            using var image = new Image<Rgba32>(width, height);
            image.Mutate(draw);
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return Convert.ToBase64String(stream.ToArray());
        }

        private static Size SizeOf(string base64)
        {
            using var image = Image.Load(Convert.FromBase64String(base64));
            return image.Size;
        }
    }
}
