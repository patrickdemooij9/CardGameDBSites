namespace SkytearHorde.Business.CustomCardMaker.Fonts
{
    public class FrutigerFontModel : FontModel
    {
        public FrutigerFontModel(string rootPath) : base(Path.Combine($"{rootPath}\\fonts\\FrutigerLTStd-Roman_0.otf"), Path.Combine($"{rootPath}\\fonts\\FrutigerLTStd-Black.otf"))
        {
            UnsupportedChars.AddRange(["ą", "ć", "ę", "ł", "ń", "ó", "ś", "ź", "ż"]);
            Fallback = new LovatoFontModel(rootPath);
        }
    }
}
