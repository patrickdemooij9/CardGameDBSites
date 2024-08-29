namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardTag
    {
        public string Name { get; set; }
        public string Color { get; set; }

        public CardTag(string name)
        {
            Name = name;
        }
    }
}
