namespace SkytearHorde.Entities.Models.ViewModels
{
    public class FilterViewModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public List<FilterItemViewModel> Items { get; set; }
        public bool IsInline { get; set; }
        public string? ApiEndpoint { get; set; }

        public FilterViewModel(string name, string alias)
        {
            Name = name;
            Alias = alias;
            Items = new List<FilterItemViewModel>();
        }
    }
}
