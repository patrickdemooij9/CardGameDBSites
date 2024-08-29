using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SkytearHorde.Business.CustomCardMaker.Markdown
{
    public class IconMarkdown : IMarkdown
    {
        private readonly string _imagePath;

        public IconMarkdown(string imagePath)
        {
            _imagePath = imagePath;
        }

        public RectangleF GetPos(float currentX, float currentY, RectangleF bounds)
        {
            var iconSize = 35;

            // Check if we are outside bounds
            if (currentX + iconSize > bounds.Width)
            {
                return new RectangleF(0, currentY + 30, iconSize, iconSize);
            }
            return new RectangleF(currentX, currentY, iconSize, iconSize);
        }

        public async Task Render(Image image, PointF pos)
        {
            using var loadedImage = await Image.LoadAsync(_imagePath);
            loadedImage.Mutate(i => i.Resize(30, 30));

            image.Mutate(it => it.DrawImage(loadedImage, new Point((int)pos.X + 5, (int)(pos.Y - 5)), 1));
        }
    }
}
