using SixLabors.ImageSharp;

namespace SkytearHorde.Business.CustomCardMaker
{
    public interface ICardLayer
    {
        Task Render(Image image);
    }
}
