using SkytearHorde.Entities.Interfaces;

namespace SkytearHorde.Entities.Models.Business
{
    public class RandomizeRequest
    {
        public int[] SetIds { get; set; }
        public ISquadRequirement[] Requirements { get; set; }

        public string? ReturnDistinctAttribute { get; set; }

        public RandomizeRequest()
        {
            Requirements = [];
            SetIds = [];
        }
    }
}
