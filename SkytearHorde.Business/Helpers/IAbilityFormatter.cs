using Umbraco.Cms.Core.Strings;

namespace SkytearHorde.Business.Helpers
{
    public interface IAbilityFormatter
    {
        IHtmlEncodedString TranslateSpecialChars(string value);
        IHtmlEncodedString TranslateSpecialCharsToMarkdown(string value);
        IHtmlEncodedString TranslateSpecialCharsToReddit(string value);
    }
}
