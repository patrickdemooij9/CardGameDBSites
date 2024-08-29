using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Hosting;
using System.Numerics;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Extensions;
using SixLabors.ImageSharp.Processing;

namespace SkytearHorde.Business.CustomCardMaker
{
    public class CardMaker
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HttpClient _httpClient;

        public CardMaker(IWebHostEnvironment webHostEnvironment, HttpClient httpClient)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpClient = httpClient;
        }

        public async Task<byte[]> Generate(RenderCardPostModel renderCardPostModel)
        {
            using var image = new Image<Rgba32>(744, 1039);

            var layers = new List<ICardLayer>();
            if (!string.IsNullOrWhiteSpace(renderCardPostModel.ImageUrl))
            {
                layers.Add(new CardImageLayer(renderCardPostModel.ImageUrl.IfNullOrWhiteSpace(Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\TestImage.png")), new Point(40, 0), new Size(670, 590), _httpClient));
            }
            layers.Add(GetCardTemplate(renderCardPostModel));

            if (!string.IsNullOrWhiteSpace(renderCardPostModel.Name))
                layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Lovato Bold.otf"), new Vector2(370, 610), renderCardPostModel.Name, 40, Color.White));

            if (renderCardPostModel.Mana.HasValue)
                layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Lovato Bold.otf"), new Vector2(75, 85), renderCardPostModel.Mana.Value.ToString(), 64, Color.White));

            var grouping = renderCardPostModel.Grouping.IfNullOrWhiteSpace("Ally");
            if (grouping == "Ally")
            {
                if (renderCardPostModel.Attack.HasValue)
                    layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Lovato Bold.otf"), new Vector2(77, 970), renderCardPostModel.Attack.Value.ToString(), 64, Color.White));

                if (renderCardPostModel.Health.HasValue)
                    layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Lovato Bold.otf"), new Vector2(670, 965), renderCardPostModel.Health.Value.ToString(), 64, Color.White));
            }

            if (!string.IsNullOrWhiteSpace(renderCardPostModel.Grouping) || !string.IsNullOrWhiteSpace(renderCardPostModel.Subtype))
            {
                var values = string.Join(" · ", new string?[] { renderCardPostModel.Grouping?.ToUpper(), renderCardPostModel.Subtype?.ToUpper() }.WhereNotNull());

                layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Lovato Bold.otf"), new Vector2(370, 670), values, 26, Color.Black));
            }

            if (!string.IsNullOrWhiteSpace(renderCardPostModel.Ability))
            {
                var icons = new Dictionary<string, string>
                {
                    { "a+", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\attackPlus.png") },
                    { "a-", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\attackMin.png") },
                    { "h+", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\healthPlus.png") },
                    { "h-", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\healthMin.png") },
                    { "ta+", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\tempAttackPlus.png") },
                    { "ta-", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\tempAttackMin.png") },
                    { "ar+", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\armorPlus.png") },
                    { "ar-", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\armorMin.png") },
                    { "tar+", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\tempArmorPlus.png") },
                    { "tar-", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\tempArmorMin.png") },
                    { "l", Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\lightning.png") }
                };

                layers.Add(new CardMarkdownLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\FrutigerLTStd-Roman_0.otf"), Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\FrutigerLTStd-Black.otf"), renderCardPostModel.Ability, 26, Color.Black, icons, new RectangleF(120, 700, 500, 200)));
            }

            if (!string.IsNullOrWhiteSpace(renderCardPostModel.MadeBy))
            {
                layers.Add(new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\FrutigerLTStd-Roman_0.otf"), new Vector2(700, 870), renderCardPostModel.MadeBy, 18, Color.Gray, rotation: 275));
            }

            layers.Add(new CardImageLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\{GetRarityImage(renderCardPostModel.Rarity)}.png"), new Point(330, 955), new Size(80, 80), _httpClient));

            foreach (var layer in layers)
            {
                await layer.Render(image);
            }

            using var memoryStream = new MemoryStream();
            await image.SaveAsPngAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private CardImageLayer GetCardTemplate(RenderCardPostModel model)
        {
            var grouping = model.Grouping ?? "Ally";
            var faction = model.Faction ?? "Liothan";

            string imageName;
            var prefix = grouping == "Ally" ? "Ally" : "AllySpell";
            switch (faction)
            {
                case "Liothan":
                default:
                    imageName = $"{prefix}Liothan";
                    break;
                case "Kurumo":
                    imageName = $"{prefix}Kurumo";
                    break;
                case "Nupten":
                    imageName = $"{prefix}Nupten";
                    break;
                case "Taulot":
                    imageName = $"{prefix}Taulot";
                    break;
            }

            return new CardImageLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\skytearhorde\\{imageName}.png"), new Point(0, 0), null, _httpClient);
        }

        private string GetRarityImage(int rarity)
        {
            switch (rarity)
            {
                case 0:
                    return "StoneBase";
                case 1:
                    return "StoneRare";
                case 2:
                    return "StoneLegendary";
                case 3:
                default:
                    return "StoneMythic";
            }
        }

        // SWU
        /*public async Task<byte[]> Generate()
        {
            using var image = new Image<Rgba32>(467, 650);

            var layers = new List<ICardLayer>
            {
                new CardImageLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\TestImage.png"), new Point(48, 98)),
                new CardImageLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\images\\CardLayer1.png"), new Point(0, 0)),
                new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Barlow-ExtraBold.ttf"), new Vector2(130, 31), "UNIT", 12),
                new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Barlow-ExtraBold.ttf"), new Vector2(315, 31), "GROUND", 12),
                new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Barlow-ExtraBold.ttf"), new Vector2(260, 56), "Lieutenant Childsen", 26),
                new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Barlow-Medium.ttf"), new Vector2(260, 83), "Death Star Prison Warden", 12),
                new CardTextLayer(Path.Combine($"{_webHostEnvironment.WebRootPath}\\fonts\\Barlow-ExtraBold.ttf"), new Vector2(240, 424), "Imperial · Official", 18)
            };

            foreach(var layer in layers)
            {
                layer.Render(image);
            }

            using var memoryStream = new MemoryStream();
            await image.SaveAsPngAsync(memoryStream);
            return memoryStream.ToArray();
        }*/
    }
}
