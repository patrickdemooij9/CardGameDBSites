using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.ObjectPool;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using static Lucene.Net.Util.Fst.Util;

namespace SkytearHorde.Business.CustomCardMaker
{
    public class CardTextLayer : ICardLayer
    {
        private readonly string _fontPath;
        private readonly Vector2 _origin;
        private readonly string _text;
        private readonly int _fontSize;
        private readonly Color _color;
        private readonly RectangleF? _bounds;
        private readonly float? _rotation;

        public CardTextLayer(string fontPath, Vector2 origin, string text, int fontSize, Color color, RectangleF? bounds = null, float? rotation = null)
        {
            _fontPath = fontPath;
            _origin = origin;
            _text = text;
            _fontSize = fontSize;
            _color = color;
            _bounds = bounds;
            _rotation = rotation;
        }

        public Task Render(Image image)
        {
            var options = new RichTextOptions(GetFont(_fontSize))
            {
                Origin = _origin,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                LineSpacing = 1.2f
            };
            if (_bounds.HasValue)
            {
                options.WrappingLength = _bounds.Value.Width;
                options.LayoutMode = LayoutMode.HorizontalTopBottom;
                options.VerticalAlignment = VerticalAlignment.Top;
            }

            image.Mutate(it =>
            {
                if (_rotation.HasValue)
                {
                    it.SetDrawingTransform(Matrix3x2Extensions.CreateRotationDegrees(_rotation.Value, _origin));
                }
                if (_bounds != null)
                {
                    it.Clip(new RectangularPolygon(_bounds.Value), a => a.DrawText(options, _text, Brushes.Solid(_color)));
                }
                else
                {
                    it.DrawText(options, _text, Brushes.Solid(_color));
                }
            });
            return Task.CompletedTask;
        }

        private Font GetFont(float size)
        {
            var fontCollection = new FontCollection();
            var family = fontCollection.Add(_fontPath);
            return family.CreateFont(size);
        }
    }
}
