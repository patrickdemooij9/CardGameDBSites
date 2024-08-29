namespace SkytearHorde.Entities.Models.ViewModels
{
    public class HeaderViewModel
    {
        public string Title { get; }
        public HeaderSize Size { get; set; }

        public HeaderViewModel(string title, HeaderSize size = HeaderSize.H1)
        {
            Title = title;
            Size = size;
        }
    }

    public enum HeaderSize
    {
        H1,
        H2,
        H3,
        H4,
        H5,
        H6
    }
}
