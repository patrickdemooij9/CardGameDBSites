namespace SkytearHorde.Entities.Models.ViewModels
{
    public class SetViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public SetViewModel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
