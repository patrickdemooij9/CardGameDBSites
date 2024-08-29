using SixLabors.ImageSharp;

namespace SkytearHorde.Business.CustomCardMaker.Markdown
{
    public interface IMarkdown
    {
        RectangleF GetPos(float currentX, float currentY, RectangleF bounds);
        Task Render(Image image, PointF pos);
    }
}
