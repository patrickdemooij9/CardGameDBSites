namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DropdownLinksModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string? Icon { get; set; }
        public Dictionary<string, string> Links { get; set; }

        public DropdownLinksModel()
        {
            Links = new Dictionary<string, string>();
        }
    }
}
