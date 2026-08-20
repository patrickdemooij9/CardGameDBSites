namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class MetaTierConfig
    {
        public int DeltaWeeks { get; set; } = 1;

        public int MinDecks { get; set; } = 10;
        public int MinEvents { get; set; } = 3;
        public int MinDecksForDelta { get; set; } = 8;

        public double PresenceWeight { get; set; } = 1.0;
        public double ConversionWeight { get; set; } = 0.0;

        public double STierScore { get; set; } = 5.0;
        public double ATierScore { get; set; } = 2.5;
        public double BTierScore { get; set; } = 0.0;
        public double CTierScore { get; set; } = -3.0;

        public MetaTierConfig Clamp()
        {
            DeltaWeeks = Math.Clamp(DeltaWeeks, 1, 8);
            MinDecks = Math.Clamp(MinDecks, 5, 500);
            MinEvents = Math.Clamp(MinEvents, 1, 50);
            MinDecksForDelta = Math.Clamp(MinDecksForDelta, 1, 500);
            PresenceWeight = Math.Clamp(PresenceWeight, 0d, 5d);
            ConversionWeight = Math.Clamp(ConversionWeight, 0d, 5d);

            // Non-monotonic thresholds would make the tier boundaries meaningless rather than merely
            // wrong, so fall back to the defaults wholesale instead of trying to repair them.
            if (!(STierScore >= ATierScore && ATierScore >= BTierScore && BTierScore >= CTierScore))
            {
                STierScore = 5.0;
                ATierScore = 2.5;
                BTierScore = 0.0;
                CTierScore = -3.0;
            }

            return this;
        }
    }
}
