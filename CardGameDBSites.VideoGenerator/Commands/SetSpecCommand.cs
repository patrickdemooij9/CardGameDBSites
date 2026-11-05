using CardGameDBSites.VideoGenerator.Api;
using CardGameDBSites.VideoGenerator.Specs;
using System.Globalization;
using System.Text.Json;

namespace CardGameDBSites.VideoGenerator.Commands;

public static class SetSpecCommand
{
    public static async Task<int> Run(string[] args)
    {
        var setKey = args[1];
        var outputPath = args[2];
        var api = new SiteApiClient(CommandArgs.Option(args, "--api") ?? CommandArgs.DefaultApiUrl);

        var sets = await api.GetSets();
        var set = sets.FirstOrDefault(s => s.Id.ToString() == setKey || string.Equals(s.Code, setKey, StringComparison.OrdinalIgnoreCase));
        if (set is null)
        {
            Console.Error.WriteLine($"Unknown set '{setKey}'. Known sets: {string.Join(", ", sets.Select(s => $"{s.Code} ({s.DisplayName})"))}");
            return 1;
        }

        var variantName = CommandArgs.Option(args, "--variant") ?? "normal";
        VariantTypeDto? variant = null;
        if (!variantName.Equals("normal", StringComparison.OrdinalIgnoreCase))
        {
            var variantTypes = await api.GetVariantTypes();
            variant = variantTypes.FirstOrDefault(v => v.DisplayName.Equals(variantName, StringComparison.OrdinalIgnoreCase) || v.Id.ToString() == variantName);
            if (variant is null)
            {
                Console.Error.WriteLine($"Unknown variant '{variantName}'. Known variants: normal, {string.Join(", ", variantTypes.Select(v => v.DisplayName))}");
                return 1;
            }
        }

        var cards = await api.GetSetCards(set.Id, variant?.Id ?? SiteApiClient.NormalVariantTypeId);
        var spec = new SetSpecBuilder().Build(set, variant, cards, CommandArgs.Option(args, "--hook"));
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(spec, SpecJson.Options));

        var total = spec.Cards.Sum(c => c.Price).ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        Console.WriteLine($"Wrote {spec.Title} to {outputPath}: {spec.Cards.Count} cards, total {total}");
        var unpriced = cards.Where(c => c.MarketPrice is null).Select(c => c.DisplayName).ToList();
        if (unpriced.Count > 0)
        {
            Console.WriteLine($"Left out {unpriced.Count} cards without a price: {string.Join(", ", unpriced)}");
        }
        return 0;
    }
}
