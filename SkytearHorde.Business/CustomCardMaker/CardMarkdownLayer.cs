using J2N.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SkytearHorde.Business.CustomCardMaker.Markdown;
using System.Numerics;
using System.Text;
using Umbraco.Extensions;

namespace SkytearHorde.Business.CustomCardMaker
{
    public class CardMarkdownLayer : ICardLayer
    {
        private readonly FontModel _font;

        private readonly string _text;
        private readonly int _fontSize;
        private readonly Color _color;
        private readonly Dictionary<string, string> _icons;
        private readonly RectangleF _bounds;

        public CardMarkdownLayer(FontModel font, string text, int fontSize, Color color, Dictionary<string, string> icons, RectangleF bounds)
        {
            _font = font;
            if (_font.Fallback != null && _font.UnsupportedChars.Any(text.Contains))
            {
                _font = _font.Fallback;
            }

            _text = text;
            _fontSize = fontSize;
            _color = color;
            _icons = icons;
            _bounds = bounds;
        }

        public async Task Render(Image image)
        {
            var index = 0;
            var markdowns = new List<IMarkdown>();
            var sBuilder = new StringBuilder();

            var boldBuilding = false;
            var commandBuilding = false;

            while (index < _text.Length)
            {
                var character = _text[index];

                if (character == '|')
                {
                    if (boldBuilding)
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetBoldFont(_fontSize)));
                    }
                    else
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetFont(_fontSize)));
                    }

                    boldBuilding = !boldBuilding;
                    sBuilder = new StringBuilder();
                }
                else if (character == '[' && !commandBuilding)
                {
                    if (boldBuilding)
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetBoldFont(_fontSize)));
                    }
                    else
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetFont(_fontSize)));
                    }

                    commandBuilding = true;
                    sBuilder = new StringBuilder();
                }
                else if (character == ']' && commandBuilding)
                {
                    var command = sBuilder.ToString();
                    if (_icons.TryGetValue(command, out string? value))
                    {
                        markdowns.Add(new IconMarkdown(value));
                    }

                    commandBuilding = false;
                    sBuilder = new StringBuilder();
                }
                else if (character == ' ')
                {
                    if (boldBuilding)
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetBoldFont(_fontSize)));
                    }
                    else
                    {
                        markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetFont(_fontSize)));
                    }
                    markdowns.Add(new TextMarkdown(" ", GetFont(_fontSize)));

                    sBuilder = new StringBuilder();
                }
                else if (character == '\r' || character == '\n')
                {
                    if (character == '\n')
                    {
                        if (boldBuilding)
                        {
                            markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetBoldFont(_fontSize)));
                        }
                        else
                        {
                            markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetFont(_fontSize)));
                        }
                        markdowns.Add(new NewlineMarkdown());
                        sBuilder = new StringBuilder();
                    }
                }
                else
                {
                    sBuilder.Append(character);
                }

                index++;
            }

            if (sBuilder.Length > 0)
            {
                markdowns.Add(new TextMarkdown(sBuilder.ToString(), GetFont(_fontSize)));
            }

            float x = 0;
            float y = 0;

            var rects = new Dictionary<IMarkdown, RectangleF>();

            foreach (var markdown in markdowns)
            {
                var pos = markdown.GetPos(x, y, _bounds);

                x += pos.Width;
                if (y != pos.Y)
                {
                    x = pos.Width;
                    y = pos.Y;
                }

                rects.Add(markdown, pos);
            }

            var lines = rects.Values.GroupBy(it => it.Y).ToDictionary(it => it.Key, it => it);
            var maxY = rects.Values.Max(it => it.Y);

            y = _bounds.Y + (_bounds.Height - maxY) / 2;

            foreach (var pair in rects)
            {
                var maxX = lines[pair.Value.Y].Max(it => it.X + it.Width);
                x = _bounds.X + (_bounds.Width - maxX) / 2;

                var pos = new PointF(x + pair.Value.X, y + pair.Value.Y);
                await pair.Key.Render(image, pos);
            }
        }

        private Font GetFont(float size)
        {
            var fontCollection = new FontCollection();
            var family = fontCollection.Add(_font.NormalFont);
            return family.CreateFont(size);
        }

        private Font GetBoldFont(float size)
        {
            var fontCollection = new FontCollection();
            var family = fontCollection.Add(_font.BoldFont);
            return family.CreateFont(size);
        }
    }
}
