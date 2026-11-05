using CardGameDBSites.VideoGenerator.Commands;

if (args is ["deck-spec", _, _, ..]) return await DeckSpecCommand.Run(args);
if (args is ["set-spec", _, _, ..]) return await SetSpecCommand.Run(args);
if (args is [not ("deck-spec" or "set-spec"), _, ..]) return await RenderCommand.Run(args);

Console.WriteLine("Usage:");
Console.WriteLine("  VideoGenerator <spec.json> <output.mp4 | frames-directory> [--frames] [--ffmpeg <path>]");
Console.WriteLine("  VideoGenerator deck-spec <deckId> <spec.json> [--hook \"<opening line>\"] [--api <base url>]");
Console.WriteLine("  VideoGenerator set-spec <setCode | setId> <spec.json> [--variant normal|Showcase|...] [--hook \"<opening line>\"] [--api <base url>]");
Console.WriteLine();
Console.WriteLine("  --frames   write PNG frames into the output directory instead of encoding an mp4");
return 1;
