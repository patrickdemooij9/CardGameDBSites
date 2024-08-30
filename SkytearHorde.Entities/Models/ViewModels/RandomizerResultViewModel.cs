namespace SkytearHorde.Entities.Models.ViewModels
{
    public class RandomizerResultViewModel
    {
        public Dictionary<string, string> Result { get; set; }

        public RandomizerResultViewModel()
        {
            Result = new Dictionary<string, string>();
        }
    }
}
