using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Detection;
using SkytearHorde.Business.Extraction;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Entities.Models.Database;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SkytearHorde.Business.Services
{
    public class CardImportQueueService
    {
        private readonly CardImportQueueRepository _queueRepository;
        private readonly CardService _cardService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CardImportQueueService> _logger;

        private static readonly IPropertyDetector AspectDetector = new ColorAspectDetector();

        public CardImportQueueService(
            CardImportQueueRepository queueRepository,
            CardService cardService,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CardImportQueueService> logger)
        {
            _queueRepository = queueRepository;
            _cardService = cardService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Resolves a configured path to an absolute one. Paths from configuration are
        /// relative to the application's content root; fully-qualified paths are used as-is.
        /// </summary>
        private string ResolveContentPath(string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
                return _webHostEnvironment.ContentRootPath;
            if (Path.IsPathFullyQualified(configuredPath))
                return configuredPath;
            return Path.Combine(_webHostEnvironment.ContentRootPath, configuredPath.TrimStart('/', '\\'));
        }

        /// <summary>
        /// Full pipeline: detect cards in the image, extract properties, deduplicate,
        /// check for variants, and write to the review queue.
        /// </summary>
        public async Task ProcessImageAsync(
            string imageBase64,
            int siteId,
            string sourceType,
            string? sourceUrl = null,
            string mimeType = "image/png")
        {
            var apiKey = _configuration["GeminiApiKey"] ?? string.Empty;
            var promptsRoot = ResolveContentPath(_configuration["CardImport:PromptsPath"] ?? "prompts");
            var stagingDir = ResolveContentPath(_configuration["CardImport:StagingPath"] ?? "card-staging");
            Directory.CreateDirectory(stagingDir);

            // Step 1: Detect and normalize cards from the reveal image
            var gameConfig = LoadGameConfig(promptsRoot, "StarWarsUnlimited");
            var extractor = new CardExtractor(gameConfig.CardDetectionPrompt);
            var extractedCards = await extractor.ExtractAsync(apiKey, imageBase64, mimeType);

            if (extractedCards.Count == 0)
            {
                _logger.LogInformation("No cards detected in image from {SourceType} {SourceUrl}", sourceType, sourceUrl);
                return;
            }

            foreach (var card in extractedCards)
            {
                await ProcessSingleCardAsync(card.Base64, siteId, sourceType, sourceUrl, apiKey, promptsRoot, stagingDir, gameConfig);
            }
        }

        private async Task ProcessSingleCardAsync(
            string cardBase64,
            int siteId,
            string sourceType,
            string? sourceUrl,
            string apiKey,
            string promptsRoot,
            string stagingDir,
            GameConfig gameConfig)
        {
            // Step 2: Compute perceptual hash and check for near-duplicates
            var imageHash = ComputeDHash(cardBase64);
            if (_queueRepository.HashExists(siteId, imageHash))
            {
                _logger.LogInformation("Skipping near-duplicate card image (hash {Hash})", imageHash);
                return;
            }

            // Step 3: Extract card properties with AI
            Dictionary<string, string> extractedData;
            try
            {
                extractedData = await ExtractCardPropertiesAsync(cardBase64, apiKey, promptsRoot, gameConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract card properties");
                return;
            }

            // Step 4: Determine status — check if a card with this name already exists
            var status = CardImportQueueStatus.Pending;
            int? potentialDuplicateId = null;

            if (extractedData.TryGetValue("Name", out var cardName) && !string.IsNullOrWhiteSpace(cardName))
            {
                if (extractedData.TryGetValue("Subname", out var subname) && !string.IsNullOrWhiteSpace(subname))
                    cardName = $"{cardName}, {subname}";

                var (baseId, _) = FindPotentialBaseCard(siteId, cardName);
                if (baseId.HasValue)
                {
                    status = CardImportQueueStatus.PotentialVariant;
                    potentialDuplicateId = baseId;
                }
            }

            // Step 5: Save image to staging directory
            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.png";
            var imagePath = Path.Combine(stagingDir, fileName);
            File.WriteAllBytes(imagePath, Convert.FromBase64String(cardBase64));

            // Step 6: Insert into queue
            _queueRepository.Insert(new CardImportQueueDBModel
            {
                SiteId = siteId,
                ExtractedData = JsonSerializer.Serialize(extractedData),
                Status = status,
                SourceType = sourceType,
                SourceUrl = sourceUrl,
                ImagePath = imagePath,
                ImageHash = imageHash,
                PotentialDuplicateId = potentialDuplicateId,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Queued card '{CardName}' with status {Status}", cardName, status);
        }

        private async Task<Dictionary<string, string>> ExtractCardPropertiesAsync(
            string cardBase64,
            string apiKey,
            string promptsRoot,
            GameConfig gameConfig)
        {
            var googleAI = new GoogleAI(apiKey: apiKey);
            var model = googleAI.GenerativeModel(model: Model.GeminiFlashLiteLatest);
            var generationConfig = new GenerationConfig { ResponseMimeType = "application/json" };

            // First call: identify card type
            var typeRequest = new GenerateContentRequest(gameConfig.InitialPrompt);
            typeRequest.AddPart(new InlineData { Data = cardBase64, MimeType = "image/png" });
            var typeResponse = await model.GenerateContent(typeRequest);
            var typeName = typeResponse?.Text?.Trim() ?? string.Empty;

            var typeConfig = gameConfig.Types.FirstOrDefault(t =>
                t.Type.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            if (typeConfig is null)
                throw new Exception($"Unknown card type: {typeName}");

            // Second call: extract properties (excluding cropRegion properties)
            var promptTemplate = File.ReadAllText(Path.Combine(promptsRoot, "StarWarsUnlimited", typeConfig.PromptFile));
            var propertiesBuilder = new StringBuilder();
            foreach (var prop in typeConfig.Properties.Where(p => p.Constant == null && p.CropRegion == null && string.IsNullOrEmpty(p.Templated)))
                propertiesBuilder.AppendLine($"- {prop.Alias} ({prop.Description})");
            var prompt = promptTemplate.Replace("{properties}", propertiesBuilder.ToString());

            var request = new GenerateContentRequest(prompt);
            request.GenerationConfig = generationConfig;
            request.AddPart(new InlineData { Data = cardBase64, MimeType = "image/png" });
            var response = await model.GenerateContent(request);

            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(response?.Text ?? "{}") ?? [];

            // Apply value mappings to the AI output (case-insensitive exact match on "from").
            foreach (var prop in typeConfig.Properties.Where(p => p.Mapping is { Count: > 0 }))
            {
                if (result.TryGetValue(prop.Alias, out var val) && !string.IsNullOrWhiteSpace(val))
                {
                    var mapped = prop.Mapping!.FirstOrDefault(m => m.From.Equals(val, StringComparison.OrdinalIgnoreCase));
                    if (mapped is not null) result[prop.Alias] = mapped.To;
                }
            }

            // Apply constants
            foreach (var prop in typeConfig.Properties.Where(p => p.Constant.HasValue))
                result[prop.Alias] = prop.Constant!.Value.ToString();

            // Crop-region properties: use local detectors
            foreach (var prop in typeConfig.Properties.Where(p => p.CropRegion != null && p.Constant == null))
            {
                var croppedBase64 = CropImageToBase64(cardBase64, prop.CropRegion!);
                if (prop.Alias == "Aspects_multiple")
                {
                    var detected = AspectDetector.Detect(croppedBase64);
                    if (detected != null) result[prop.Alias] = detected;
                }
            }

            // Post-process: TitleCase and Split
            foreach (var prop in typeConfig.Properties.Where(p => p.ToTitleCase))
            {
                if (result.TryGetValue(prop.Alias, out var val) && !string.IsNullOrWhiteSpace(val))
                    result[prop.Alias] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val.ToLower());
            }
            foreach (var prop in typeConfig.Properties.Where(p => !string.IsNullOrWhiteSpace(p.Split)))
            {
                if (result.TryGetValue(prop.Alias, out var val))
                    result[prop.Alias] = string.Join(",", val.Split(prop.Split!)
                        .Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.Trim().ToLower())));
            }

            return result;
        }

        /// <summary>
        /// Returns the aliases of the read-only, templated fields for a given card type.
        /// These are computed on approval rather than read by the AI.
        /// </summary>
        public IReadOnlyList<string> GetTemplatedFields(string? cardType)
        {
            if (string.IsNullOrWhiteSpace(cardType)) return [];

            var typeConfig = FindTypeConfig(cardType);
            if (typeConfig is null) return [];

            return typeConfig.AllProperties
                .Where(p => !string.IsNullOrEmpty(p.Templated))
                .Select(p => p.Alias)
                .ToList();
        }

        /// <summary>
        /// Computes the templated fields for the card and merges them into a copy of the supplied data.
        /// Tokens: {set} or {expansion} = the set short code; {prop:Alias} or {prop:Alias:Format} = another field's value.
        /// </summary>
        public Dictionary<string, string> ApplyTemplates(IDictionary<string, string> data, string setShortCode)
        {
            var result = new Dictionary<string, string>(data, StringComparer.Ordinal);
            if (!TryGetIgnoreCase(result, "Card Type", out var cardType)) return result;

            var typeConfig = FindTypeConfig(cardType);
            if (typeConfig is null) return result;

            foreach (var prop in typeConfig.AllProperties.Where(p => !string.IsNullOrEmpty(p.Templated)))
                result[prop.Alias] = ResolveTemplate(prop.Templated!, result, setShortCode);

            return result;
        }

        private static readonly Regex TemplateTokenRegex = new(@"\{([^}]+)\}", RegexOptions.Compiled);

        private static string ResolveTemplate(string template, IDictionary<string, string> data, string setShortCode)
        {
            return TemplateTokenRegex.Replace(template, match =>
            {
                var token = match.Groups[1].Value.Trim();

                if (token.Equals("set", StringComparison.OrdinalIgnoreCase) ||
                    token.Equals("expansion", StringComparison.OrdinalIgnoreCase))
                    return setShortCode;

                if (token.StartsWith("prop:", StringComparison.OrdinalIgnoreCase))
                {
                    var rest = token["prop:".Length..];
                    var parts = rest.Split(':', 2);
                    var alias = parts[0].Trim();
                    var format = parts.Length > 1 ? parts[1].Trim() : null;

                    TryGetIgnoreCase(data, alias, out var value);
                    value ??= string.Empty;

                    if (!string.IsNullOrEmpty(format))
                    {
                        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                            return number.ToString(format, CultureInfo.InvariantCulture);
                        try { return string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}", value); }
                        catch (FormatException) { return value; }
                    }
                    return value;
                }

                // Unknown token — leave it untouched.
                return match.Value;
            });
        }

        private CardTypeConfig? FindTypeConfig(string cardType)
        {
            return LoadDefaultGameConfig().Types.FirstOrDefault(t => t.Type.Equals(cardType, StringComparison.OrdinalIgnoreCase));
        }

        private GameConfig LoadDefaultGameConfig()
        {
            var promptsRoot = ResolveContentPath(_configuration["CardImport:PromptsPath"] ?? "prompts");
            return LoadGameConfig(promptsRoot, "StarWarsUnlimited");
        }

        // Control keys in a preset variant's properties that are not editable card fields.
        private static bool IsControlKey(string key) =>
            key.Equals("VariantTypeId", StringComparison.OrdinalIgnoreCase) ||
            key.Equals("image_base64", StringComparison.OrdinalIgnoreCase);

        // A preset field whose configured value starts with '{' is a template computed on approval.
        private static bool IsTemplatedValue(string? value) =>
            !string.IsNullOrEmpty(value) && value.TrimStart().StartsWith('{');

        /// <summary>
        /// Returns the variant presets configured in Config.json. A preset groups one or more variants
        /// (e.g. normal + foil) that are all created together on approval. Each variant exposes its
        /// variant type id (matched against Variant.InternalID), a display name, and its fields with a
        /// flag marking the read-only templated ones (computed on approval).
        /// </summary>
        public IReadOnlyList<ImportPreset> GetPresets()
        {
            var config = LoadDefaultGameConfig();
            var presets = new List<ImportPreset>();

            foreach (var preset in config.Presets)
            {
                var variants = new List<ImportPresetVariant>();
                foreach (var variant in preset.Variants)
                {
                    if (!variant.Properties.TryGetValue("VariantTypeId", out var rawVariantType) ||
                        !int.TryParse(rawVariantType, out var variantTypeId))
                        continue;

                    variant.Properties.TryGetValue("Name", out var name);
                    // "Name" is not an editable field for variants — the variant is always named after
                    // the preset variant itself (e.g. "Organized play foil"), so it is excluded here.
                    var fields = variant.Properties
                        .Where(kv => !IsControlKey(kv.Key)
                                  && !kv.Key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                        .Select(kv => new ImportPresetField
                        {
                            Alias = kv.Key,
                            Templated = IsTemplatedValue(kv.Value)
                        })
                        .ToList();

                    variants.Add(new ImportPresetVariant
                    {
                        VariantTypeId = variantTypeId,
                        Name = string.IsNullOrWhiteSpace(name) ? $"Variant {variantTypeId}" : name,
                        Fields = fields
                    });
                }

                if (variants.Count > 0)
                    presets.Add(new ImportPreset { Name = preset.Name, Id = preset.Id, Variants = variants });
            }

            return presets;
        }

        /// <summary>
        /// Builds the final property set for a single preset variant: editable fields take the supplied
        /// values (autofilled from the AI read on the client, else empty), and templated fields (config
        /// value starting with '{') are computed using the set short code and the other field values.
        /// </summary>
        public Dictionary<string, string> BuildVariantProperties(int variantTypeId, IDictionary<string, string> editableValues, string setShortCode)
        {
            var config = LoadDefaultGameConfig();
            var variant = config.Presets
                .SelectMany(p => p.Variants)
                .FirstOrDefault(v => v.Properties.TryGetValue("VariantTypeId", out var raw)
                                  && int.TryParse(raw, out var id) && id == variantTypeId);

            var result = new Dictionary<string, string>();
            if (variant is null)
            {
                foreach (var kv in editableValues) result[kv.Key] = kv.Value;
                return result;
            }

            var templated = new List<KeyValuePair<string, string>>();
            foreach (var (alias, configValue) in variant.Properties)
            {
                if (IsControlKey(alias)) continue;

                // A variant is always named after the preset variant, never the AI-read name.
                if (alias.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    result[alias] = configValue;
                    continue;
                }

                if (IsTemplatedValue(configValue))
                    templated.Add(new(alias, configValue));
                else
                {
                    TryGetIgnoreCase(editableValues, alias, out var value);
                    result[alias] = value ?? string.Empty;
                }
            }

            // Compute templated fields once the editable values are in place (so {prop:...} can reference them).
            foreach (var (alias, template) in templated)
                result[alias] = ResolveTemplate(template, result, setShortCode);

            return result;
        }

        /// <summary>
        /// Searches existing base cards (no variant type) for an exact display-name match.
        /// Returns the matched base card's id and display name, or (null, null) when nothing matches.
        /// </summary>
        public (int? BaseId, string? MatchedName) FindPotentialBaseCard(int siteId, string cardName)
        {
            if (string.IsNullOrWhiteSpace(cardName)) return (null, null);

            var existing = _cardService.Search(new CardSearchQuery(1, siteId)
            {
                Query = cardName,
                VariantTypeIds = [0]
            }, out _);

            if (existing.Length > 0 && existing[0].DisplayName.Equals(cardName, StringComparison.OrdinalIgnoreCase))
                return (existing[0].BaseId, existing[0].DisplayName);

            return (null, null);
        }

        private static bool TryGetIgnoreCase(IDictionary<string, string> data, string key, out string? value)
        {
            foreach (var kvp in data)
            {
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    value = kvp.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Computes a 64-bit difference hash (dHash) as a 16-char hex string.
        /// Resize to 9x8 grayscale, compare each pixel to its right neighbour per row.
        /// </summary>
        private static string ComputeDHash(string base64Image)
        {
            var bytes = Convert.FromBase64String(base64Image);
            using var image = SixLabors.ImageSharp.Image.Load<L8>(bytes); // L8 = 8-bit grayscale
            image.Mutate(ctx => ctx.Resize(9, 8));

            ulong hash = 0;
            ulong bit = 1;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (image[x, y].PackedValue > image[x + 1, y].PackedValue)
                        hash |= bit;
                    bit <<= 1;
                }
            }
            return hash.ToString("x16");
        }

        private static string CropImageToBase64(string base64Image, CropRegionConfig region)
        {
            var imageBytes = Convert.FromBase64String(base64Image);
            using var image = SixLabors.ImageSharp.Image.Load(imageBytes);

            int x = (int)(image.Width  * region.X1 / 100.0);
            int y = (int)(image.Height * region.Y1 / 100.0);
            int w = (int)(image.Width  * (region.X2 - region.X1) / 100.0);
            int h = (int)(image.Height * (region.Y2 - region.Y1) / 100.0);

            var cropped = image.Clone(ctx => ctx.Crop(new Rectangle(x, y, w, h)));
            using var stream = new MemoryStream();
            cropped.SaveAsPng(stream);
            return Convert.ToBase64String(stream.ToArray());
        }

        private static GameConfig LoadGameConfig(string promptsRoot, string gameName)
        {
            var json = File.ReadAllText(Path.Combine(promptsRoot, gameName, "Config.json"));
            return JsonSerializer.Deserialize<GameConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception($"Failed to load config for {gameName}");
        }

        // ── Minimal config models (mirrors WPF CardReader Config.json shape) ──

        private class GameConfig
        {
            public string InitialPrompt { get; set; } = string.Empty;
            public string CardDetectionPrompt { get; set; } = string.Empty;
            public List<CardTypeConfig> Types { get; set; } = [];
            public List<PresetConfig> Presets { get; set; } = [];
        }

        private class PresetConfig
        {
            public string Name { get; set; } = string.Empty;
            public int Id { get; set; }
            public List<PresetVariantConfig> Variants { get; set; } = [];
        }

        private class PresetVariantConfig
        {
            public int Id { get; set; }
            public Dictionary<string, string> Properties { get; set; } = [];
        }

        private class CardTypeConfig
        {
            public string Type { get; set; } = string.Empty;
            public string PromptFile { get; set; } = string.Empty;
            public List<CardPropertyConfig> Properties { get; set; } = [];
            public List<CardPropertyConfig> BackSideProperties { get; set; } = [];

            public IEnumerable<CardPropertyConfig> AllProperties => Properties.Concat(BackSideProperties);
        }

        private class CardPropertyConfig
        {
            public string Alias { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool ToTitleCase { get; set; }
            public string? Split { get; set; }
            // Constant can be a number (e.g. 3) or a string (e.g. "Todo"), so keep it as a raw JSON element.
            public JsonElement? Constant { get; set; }
            public CropRegionConfig? CropRegion { get; set; }
            // Optional value mappings applied to the AI output (case-insensitive exact match on "from").
            public List<MappingConfig>? Mapping { get; set; }
            // When set, the field is read-only and computed on approval from this template.
            public string? Templated { get; set; }
        }

        private class MappingConfig
        {
            public string From { get; set; } = string.Empty;
            public string To { get; set; } = string.Empty;
        }

        private class CropRegionConfig
        {
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
        }
    }

    public class ImportPreset
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public List<ImportPresetVariant> Variants { get; set; } = [];
    }

    public class ImportPresetVariant
    {
        public int VariantTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ImportPresetField> Fields { get; set; } = [];
    }

    public class ImportPresetField
    {
        public string Alias { get; set; } = string.Empty;
        public bool Templated { get; set; }
    }
}
