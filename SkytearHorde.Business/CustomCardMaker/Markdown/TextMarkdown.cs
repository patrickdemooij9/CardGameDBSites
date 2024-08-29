using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace SkytearHorde.Business.CustomCardMaker.Markdown
{
    public class TextMarkdown : IMarkdown
    {
        private readonly string _text;
        private readonly Font _font;

        public TextMarkdown(string text, Font font)
        {
            _text = text;
            _font = font;
        }

        public RectangleF GetPos(float currentX, float currentY, RectangleF bounds)
        {
            var options = new RichTextOptions(_font);
            var size = TextMeasurer.MeasureBounds(_text, options);

            // Check if we are outside bounds
            if (currentX + size.Width > bounds.Width)
            {
                return new RectangleF(0, currentY + 30, size.Width, size.Height);
            }
            return new RectangleF(currentX, currentY, size.Width, size.Height);
        }

        public Task Render(Image image, PointF pos)
        {
            var options = new RichTextOptions(_font)
            {
                Origin = new Vector2(pos.X, pos.Y)
            };

            image.Mutate(it => it.DrawText(options, _text, Color.Black));
            return Task.CompletedTask;
        }
    }
}
