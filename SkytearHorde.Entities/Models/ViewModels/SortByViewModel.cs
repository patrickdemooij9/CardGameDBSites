namespace SkytearHorde.Entities.Models.ViewModels
{
    public class SortByViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SortByViewModel(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
