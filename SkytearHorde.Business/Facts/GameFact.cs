namespace SkytearHorde.Business.Facts
{
    public class GameFact
    {
        public required string Key { get; set; }

        /// <summary>The "Did you know …?" sentence shown on the hook slide.</summary>
        public required string Hook { get; set; }

        /// <summary>The data slides that follow the hook.</summary>
        public required IReadOnlyList<FactSlide> Slides { get; set; }
    }
}
