using Microsoft.AspNetCore.Hosting;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using System.Drawing.Printing;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Exports
{
    public class PdfDeckExport : IDeckExport
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PageSize _pageSize;
        private readonly CardService _cardService;

        private int _xCount = 0;
        private int _yCount = 0;

        private PdfDocument _document;
        private XGraphics _gfx;
        private PdfPage _page;

        public PdfDeckExport(IWebHostEnvironment webHostEnvironment, PageSize pageSize, CardService cardService)
        {
            _webHostEnvironment = webHostEnvironment;
            _pageSize = pageSize;
            _cardService = cardService;
            _document = new PdfDocument();
            _page = AddNewPage();
            _gfx = XGraphics.FromPdfPage(_page);
        }

        public Task<byte[]> ExportDeck(Deck deck)
        {
            var width = XUnit.FromMillimeter(63.5);
            var height = XUnit.FromMillimeter(88.9);
            double gap = 1.3;
            double startingPointX = (_page.Width - width * 3 - gap * 2) / 2;
            double startingPointY = (_page.Height - height * 3 - gap * 2) / 2;
            foreach (var deckCard in deck.Cards)
            {
                var card = _cardService.Get(deckCard.CardId);
                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{card.Image!.Url()}");

                XImage? image = null;
                if (File.Exists(path))
                {
                    image = LoadImage(path);
                }

                for (var i = 0; i < deckCard.Amount; i++)
                {
                    var x = startingPointX + width * _xCount + gap * _xCount;
                    var y = startingPointY + height * _yCount + gap * _yCount;

                    AddImage(image, x, y, width, height);
                }

                if (card.BackImage != null)
                {
                    path = Path.Combine($"{_webHostEnvironment.WebRootPath}\\{card.BackImage!.Url()}");

                    image = null;
                    if (File.Exists(path))
                    {
                        image = LoadImage(path);
                    }

                    for (var i = 0; i < deckCard.Amount; i++)
                    {
                        var x = startingPointX + width * _xCount + gap * _xCount;
                        var y = startingPointY + height * _yCount + gap * _yCount;

                        AddImage(image, x, y, width, height);
                    }
                }
                image?.Dispose();
            }

            using (var stream = new MemoryStream())
            {
                _document.Save(stream, false);
                return Task.FromResult(stream.ToArray());
            }            
        }

        private void AddImage(XImage? image, double x, double y, double width, double height)
        {
            if (image != null)
            {
                _gfx.DrawImage(image, x, y, width, height);
            }
            else
            {
                _gfx.DrawRectangle(new XSolidBrush(XColor.FromKnownColor(XKnownColor.Red)), x, y, width, height);
            }

            _xCount++;
            if (_xCount > 2)
            {
                _xCount = 0;
                _yCount++;

                if (_yCount > 2)
                {
                    _yCount = 0;

                    _page = AddNewPage();
                    _gfx = XGraphics.FromPdfPage(_page);
                }
            }
        }

        private XImage LoadImage(string path)
        {
            using (var tempImage = Image.Load(path))
            {
                if (tempImage.Width > tempImage.Height)
                {
                    tempImage.Mutate(it => it.Rotate(RotateMode.Rotate270));
                }

                var memoryStream = new MemoryStream();
                tempImage.Save(memoryStream, tempImage.DetectEncoder(path));
                memoryStream.Position = 0;

                ImageSource.ImageSourceImpl = new ImageSharpImageSourceFix<Rgba32>();
                return XImage.FromStream(() => memoryStream);
            }
        }

        private PdfPage AddNewPage()
        {
            var page = _document.AddPage();
            var pageSize = PageSizeConverter.ToSize(_pageSize);
            page.Width = pageSize.Width;
            page.Height = pageSize.Height;

            return page;
        }
    }
}
