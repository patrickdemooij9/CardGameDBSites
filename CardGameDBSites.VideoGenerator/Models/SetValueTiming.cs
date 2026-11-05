namespace CardGameDBSites.VideoGenerator.Models;

public record SetValueTiming(
    int FramesPerSecond = 30,
    double HookSeconds = 2.5,
    double WipeSeconds = 0.6,
    double CardsSeconds = 28,
    double LastCardExtraSeconds = 1.0,
    double FinaleSeconds = 3.5);
