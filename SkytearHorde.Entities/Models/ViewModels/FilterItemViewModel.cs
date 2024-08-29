namespace SkytearHorde.Entities.Models.ViewModels
{
    public class FilterItemViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string? IconUrl { get; set; }
        public bool IsChecked { get; set; }

        public FilterItemViewModel(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
