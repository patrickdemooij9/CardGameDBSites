namespace SkytearHorde.Business.CustomCardMaker.Fonts
{
    public class LovatoFontModel : FontModel
    {
        public LovatoFontModel(string rootPath) : base(Path.Combine($"{rootPath}\\fonts\\Lovato Regular.otf"), Path.Combine($"{rootPath}\\fonts\\Lovato Bold.otf"))
        {
        }
    }
}
