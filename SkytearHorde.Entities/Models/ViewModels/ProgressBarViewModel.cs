namespace SkytearHorde.Entities.Models.ViewModels
{
    public class ProgressBarViewModel
    {
        public int PercentFilled { get; set; }
        public string? Description { get; set; }

        public ProgressBarViewModel(int percentFilled)
        {
            PercentFilled = percentFilled;
        }
    }
}
