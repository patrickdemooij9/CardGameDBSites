using SixLabors.ImageSharp;

namespace SkytearHorde.Business.CustomCardMaker.Markdown
{
    public class NewlineMarkdown : IMarkdown
    {
        public RectangleF GetPos(float currentX, float currentY, RectangleF bounds)
        {
            return new RectangleF(0, currentY + 30, 0, 0);
        }

        public Task Render(Image image, PointF pos)
        {
            return Task.CompletedTask;
        }
    }
}
