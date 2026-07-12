namespace SkytearHorde.Business.Facts
{
    public enum FactSlideKind
    {
        /// <summary>A single hero card image with a big stat.</summary>
        HeroCard,
        /// <summary>A titled list of string items (e.g. trait names).</summary>
        List
    }

    /// <summary>Flat, renderer-agnostic description of one data slide.</summary>
    public class FactSlide
    {
        public required FactSlideKind Kind { get; set; }
        public string? Heading { get; set; }
        public string? Title { get; set; }
        public string? BigValue { get; set; }
        public string? BigLabel { get; set; }
        public string? Caption { get; set; }

        /// <summary>Relative media url of the hero card (resolved to absolute by the controller).</summary>
        public string? ImageUrl { get; set; }

        public IReadOnlyList<string>? Items { get; set; }
    }
}
