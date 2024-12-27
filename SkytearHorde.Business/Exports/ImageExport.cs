using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Path = System.IO.Path;
using Image = SixLabors.ImageSharp.Image;
using File = System.IO.File;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.TTS;

namespace SkytearHorde.Business.Exports
{
    public class ImageExport : IDeckExport
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CardService _cardService;
        private readonly ISiteService _siteService;
        private readonly ImageExportConfig _config;
        private readonly Color[] _deckColors;

        public ImageExport(IWebHostEnvironment webHostEnvironment,
            CardService cardService,
            ISiteService siteService,
            ImageExportConfig config,
            Color[] deckColors)
        {
            _webHostEnvironment = webHostEnvironment;
            _cardService = cardService;
            _siteService = siteService;
            _config = config;
            _deckColors = deckColors;
        }

        public async Task<byte[]> ExportDeck(Deck deck)
        {
            var marginSides = 20;
            using var image = new Image<Rgba32>(2040, 1380);

            RenderBackground(image, deck);

            RenderDeckTitle(image, deck.Name);

            var mainCards = GetMainCards(deck);

            var imageContainerX = 2040 - marginSides * 2;
            var startingPointX = marginSides;
            if (mainCards.Length > 0)
            {
                imageContainerX -= 450;
                startingPointX += 450;
                await RenderMainCards(image, mainCards, new Size(300, 400), new Point(marginSides, 200), 10);
            }

            await RenderAllDeckImages(image, deck.Cards.Except(mainCards).ToList(), new Size(imageContainerX, 1000), new Point(startingPointX, 200), 10, mainCards.Length > 0 ? 6 : 8);
            RenderDeckLink(image, $"{_siteService.GetDeckOverview(deck.TypeId).Url(mode: UrlMode.Absolute)}{deck.Id}");

            using var memoryStream = new MemoryStream();
            await image.SaveAsPngAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private void RenderBackground(Image image, Deck deck)
        {
            var colors = _deckColors;

            if (colors.Length == 0)
            {
                image.Mutate(it => it.BackgroundColor(Color.White));
                return;
            }

            var firstColor = colors[0].ToPixel<Argb32>();
            var step = 1f / (colors.Length - 1);

            image.Mutate(it => it.Fill(new LinearGradientBrush(new Point(0, 0), new Point(2040, 1380), GradientRepetitionMode.None, colors.Select((it, index) => new ColorStop(step * index, it)).ToArray())));
        }
        private void RenderDeckTitle(Image image, string title)
        {
            var options = new RichTextOptions(GetBoldFont(84))
            {
                Origin = new System.Numerics.Vector2(1020, 50),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            image.Mutate(it => it.DrawText(options, title, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2)));
        }

        private DeckCard[] GetMainCards(Deck deck)
        {
            if (_config.MainCardLogic.Length == 0) return Array.Empty<DeckCard>();
            return deck.Cards.Where(c => _config.MainCardLogic.All(r => r.GetRequirement().IsValid(new[] { _cardService.Get(c.CardId)! }))).ToArray();
        }

        private async Task RenderMainCards(Image image, DeckCard[] mainCards, Size cardSize, Point startingPoint, int gap)
        {
            int y = startingPoint.Y;
            foreach (var deckCard in mainCards)
            {
                var card = _cardService.Get(deckCard.CardId)!;
                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{card.Image!.Url()}");
                if (!File.Exists(path)) { continue; }

                using (var tempImage = await Image.LoadAsync(path))
                {
                    if (tempImage.Size.Width >= tempImage.Size.Height)
                    {
                        tempImage.Mutate(it => it.Resize(new Size(cardSize.Height, cardSize.Width)));
                    }
                    else
                    {
                        tempImage.Mutate(it => it.Resize(cardSize));
                    }

                    image.Mutate(it => it.DrawImage(tempImage, new Point(startingPoint.X, y), 1));
                }

                y += cardSize.Height + gap;
            }
        }

        private async Task RenderAllDeckImages(Image image, List<DeckCard> cards, Size size, Point startingPoint, int gap, int maxXCards)
        {
            var groups = cards.GroupBy(it => it.GroupId).ToArray();
            if (groups.Length == 1)
            {
                var squadGroup = _config.SquadSettings.Squads.ToItems<SquadConfig>().FirstOrDefault(it => it.SquadId == groups[0].Key);
                if (squadGroup is null) return;

                await RenderDeckImagesForGroup(squadGroup, image, cards, size, startingPoint, gap, maxXCards, 3);
                return;
            }

            int x = 0;
            int y = 0;
            var maxY = Math.Max((int)Math.Ceiling(groups.Length / (double)2), 2);
            foreach (var group in groups)
            {
                var squadGroup = _config.SquadSettings.Squads.ToItems<SquadConfig>().FirstOrDefault(it => it.SquadId == group.Key);
                if (squadGroup is null) continue;
                var groupCards = group.ToArray();

                var actualSize = new Size(size.Width / 2 - 50, size.Height / maxY);
                var startingPointX = x == 0 ? startingPoint.X : startingPoint.X + actualSize.Width + 50;
                var actualStartingPoint = new Point(startingPointX, startingPoint.Y + actualSize.Height * y);
                await RenderDeckImagesForGroup(squadGroup, image, groupCards.ToList(), actualSize, actualStartingPoint, gap, groupCards.Length, 1);

                x++;
                if (x > 1)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private async Task RenderDeckImagesForGroup(SquadConfig squadConfig, Image image, List<DeckCard> deckCards, Size size, Point startingPoint, int gap, int maxXCards, int maxYCards)
        {
            int maxY = maxYCards;
            int maxX = Math.Max((int)Math.Ceiling(deckCards.Count / (double)maxY), maxXCards);
            int xSteps = (size.Width - gap * (maxX - 1)) / maxX;
            int ySteps = (size.Height - gap * 2) / maxY;

            int x = 0;
            int y = 0;

            var richText = new RichTextOptions(GetFont(48));
            var renderAsImages = squadConfig.DetailDisplayType == "CardImage";

            if (!renderAsImages)
            {
                var options = new RichTextOptions(GetBoldFont(64))
                {
                    Origin = startingPoint,
                    TextAlignment = TextAlignment.Start,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                image.Mutate(it => it.DrawText(options, squadConfig.Label!, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2)));

                startingPoint.Y += 80;
            }

            var cards = deckCards.Select(it => _cardService.Get(it.CardId)!);
            foreach (var card in new SortingHelper(cards).Sort(_config.SortOptions))
            {
                var point = new Point(startingPoint.X + x * xSteps + gap * x, startingPoint.Y + y * ySteps + gap * y);
                if (renderAsImages)
                {
                    var deckCard = deckCards.First(it => it.CardId == card.BaseId);
                    var path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{card.Image?.Url()}");
                    if (!File.Exists(path)) { continue; }

                    using (var tempImage = await Image.LoadAsync(path))
                    {
                        tempImage.Mutate(it => it.Resize(xSteps, ySteps));

                        image.Mutate(it => it.DrawImage(tempImage, point, 1));
                    }

                    if (_config.ShowCardAmounts)
                    {
                        var circleRadius = 25;
                        var circleX = startingPoint.X + x * xSteps + gap * x + xSteps / 2;
                        var circleY = startingPoint.Y + y * ySteps + gap * y + ySteps - circleRadius / 2;
                        var circle = new EllipsePolygon(circleX, circleY, circleRadius);

                        richText.Origin = new System.Numerics.Vector2(circleX, circleY);
                        richText.HorizontalAlignment = HorizontalAlignment.Center;
                        richText.VerticalAlignment = VerticalAlignment.Center;

                        image.Mutate(it => it.Fill(new DrawingOptions(), Color.White, new EllipsePolygon(circleX, circleY, circleRadius + 2)));
                        image.Mutate(it => it.Fill(new DrawingOptions(), Color.Black, circle));
                        image.Mutate(it => it.DrawText(richText, deckCard.Amount.ToString(), Brushes.Solid(Color.White)));
                    }
                }
                else
                {
                    var options = new RichTextOptions(GetFont(42))
                    {
                        Origin = point,
                        TextAlignment = TextAlignment.Start,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    image.Mutate(it => it.DrawText(options, card.DisplayName, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 1.5f)));
                }

                x += 1;
                if (x >= maxX)
                {
                    x = 0;
                    y += 1;
                }
            }
        }

        private void RenderDeckLink(Image image, string link)
        {
            image.Mutate(it => it.Fill(Color.Black, new RectangularPolygon(0, 1300, 2040, 180)));

            var options = new RichTextOptions(GetFont(48))
            {
                Origin = new System.Numerics.Vector2(1020, 1310),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            image.Mutate(it => it.DrawText(options, link, Brushes.Solid(Color.White)));
        }

        private Font GetFont(float size)
        {
            var fontCollection = new FontCollection();
            var family = fontCollection.Add(Path.Combine($"{_webHostEnvironment.WebRootPath}\\/fonts/OpenSans-Regular.ttf"));
            return family.CreateFont(size, FontStyle.Bold);
        }

        private Font GetBoldFont(float size)
        {
            var fontCollection = new FontCollection();
            var family = fontCollection.Add(Path.Combine($"{_webHostEnvironment.WebRootPath}\\/fonts/OpenSans-Bold.ttf"));
            return family.CreateFont(size, FontStyle.Bold);
        }
    }
}
