using SkytearHorde.Business.Helpers;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class MarkdownHelperTests
    {
        [Test]
        public void ToHtml_BasicMarkdown_ConvertsToHtml()
        {
            var result = MarkdownHelper.ToHtml("**bold** text");

            Assert.That(result, Does.Contain("<strong>bold</strong>"));
            Assert.That(result, Does.Contain("text"));
        }

        [Test]
        public void ToHtml_NullInput_ReturnsEmptyString()
        {
            var result = MarkdownHelper.ToHtml(null!);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToHtml_EmptyString_ReturnsEmptyString()
        {
            var result = MarkdownHelper.ToHtml("");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToHtml_WhitespaceOnly_ReturnsEmptyString()
        {
            var result = MarkdownHelper.ToHtml("   ");

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToHtml_ItalicText_ConvertsToItalic()
        {
            var result = MarkdownHelper.ToHtml("*italic*");

            Assert.That(result, Does.Contain("<em>italic</em>"));
        }

        [Test]
        public void ToHtml_Link_ConvertsToAnchorTag()
        {
            var result = MarkdownHelper.ToHtml("[text](https://example.com)");

            Assert.That(result, Does.Contain("<a"));
            Assert.That(result, Does.Contain("nofollow"));
            Assert.That(result, Does.Contain("text</a>"));
        }

        [Test]
        public void ToHtml_LineBreaks_ConvertsToBreaks()
        {
            var result = MarkdownHelper.ToHtml("line1\nline2");

            Assert.That(result, Does.Contain("</br>"));
        }

        [Test]
        public void ToHtml_DisablesHtml_TagsEscaped()
        {
            var result = MarkdownHelper.ToHtml("<script>alert('xss')</script>");

            Assert.That(result, Does.Not.Contain("<script>"));
        }

        [Test]
        public void ToHtml_PlainText_ReturnsParagraph()
        {
            var result = MarkdownHelper.ToHtml("hello world");

            Assert.That(result, Does.Contain("hello world"));
        }

        [Test]
        public void ToHtml_MultipleBoldSpots_AllConverted()
        {
            var result = MarkdownHelper.ToHtml("**one** and **two**");

            Assert.That(result, Does.Contain("<strong>one</strong>"));
            Assert.That(result, Does.Contain("<strong>two</strong>"));
        }
    }
}
