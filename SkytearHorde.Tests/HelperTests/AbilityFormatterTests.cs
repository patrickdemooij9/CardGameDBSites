using SkytearHorde.Business.Helpers;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class AbilityFormatterTests
    {
        [Test]
        public void TranslateSpecialCharsToReddit_ReplacesLineBreaks()
        {
            var formatter = CreateFormatter();
            var input = "line1\\rline2";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            Assert.That(result.ToString(), Does.Contain("line1\nline2"));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_ReplacesPipeWithBold()
        {
            var formatter = CreateFormatter();
            var input = "some|text|here";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            Assert.That(result.ToString(), Does.Contain("some**text**here"));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_EmptyString_ReturnsEmpty()
        {
            var formatter = CreateFormatter();

            var result = formatter.TranslateSpecialCharsToReddit("");

            Assert.That(result.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_NoSpecialChars_ReturnsUnchanged()
        {
            var formatter = CreateFormatter();
            var input = "plain text";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            Assert.That(result.ToString(), Is.EqualTo("plain text"));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_BothSpecialChars_ReplacesAll()
        {
            var formatter = CreateFormatter();
            var input = "line1\\r|bold text|";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            Assert.That(result.ToString(), Does.Contain("\n"));
            Assert.That(result.ToString(), Does.Contain("**bold text**"));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_MultipleLineBreaks_AllReplaced()
        {
            var formatter = CreateFormatter();
            var input = "a\\rb\\rc";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            Assert.That(result.ToString(), Is.EqualTo("a\nb\nc"));
        }

        [Test]
        public void TranslateSpecialCharsToReddit_OddPipeCount_AllPipesReplaced()
        {
            var formatter = CreateFormatter();
            var input = "|a|b|c|";

            var result = formatter.TranslateSpecialCharsToReddit(input);

            // String.Replace replaces ALL occurrences: |a|b|c| -> **a**b**c**
            Assert.That(result.ToString(), Does.Contain("**a**b**c**"));
        }

        private static AbilityFormatter CreateFormatter()
        {
            return new AbilityFormatter(null!);
        }
    }
}
