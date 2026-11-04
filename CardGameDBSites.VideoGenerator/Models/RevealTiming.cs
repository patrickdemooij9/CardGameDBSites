namespace CardGameDBSites.VideoGenerator.Models;

public record RevealTiming(
    int FramesPerSecond = 30,
    double IntroSeconds = 0.5,
    double HookSeconds = 2.5,
    double TransitionSeconds = 0.6,
    double HoldSeconds = 1.6,
    double OutroSeconds = 0.5,
    double FinaleStaggerSeconds = 1.6,
    double FinaleFlySeconds = 0.5,
    double FinaleHoldSeconds = 3.0);
