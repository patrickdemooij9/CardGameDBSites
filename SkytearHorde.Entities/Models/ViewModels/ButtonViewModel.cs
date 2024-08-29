namespace SkytearHorde.Entities.Models.ViewModels
{
    public class ButtonViewModel
    {
        public string Text { get; set; }
        public ButtonSize Size { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public ButtonViewModel(string text)
        {
            Text = text;
            Size = ButtonSize.Medium;
            Attributes = new Dictionary<string, string>();
        }
    }

    public enum ButtonSize
    {
        Small,
        Medium,
        Large
    }
}
