using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Helpers
{
    public class AbilityFormatter : IAbilityFormatter
    {
        private readonly SettingsService _settingsService;

        public AbilityFormatter(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public IHtmlEncodedString TranslateSpecialChars(string value)
        {
            var text = value.Replace("\\r", "</p><p>");
            foreach(var keyword in _settingsService.GetSiteSettings().Keywords)
            {
                text = text.Replace($"[{keyword.Keyword}]", $"<img src=\"{keyword.Image}\" class=\"keyword-icon\"/>");
            }
            var isStart = true;
            while (text.Contains('|'))
            {
                text = text.ReplaceFirst("|", isStart ? "<b>" : "</b>");
                isStart = !isStart;
            }

            return new HtmlEncodedString(text);
        }

        public IHtmlEncodedString TranslateSpecialCharsToMarkdown(string value)
        {
            var text = value;

            foreach (var keyword in _settingsService.GetSiteSettings().Keywords)
            {
                text = text.Replace($"[{keyword.Keyword}]", $"{keyword.DiscordIcon}");
            }
            text = text.Replace("\\r", "\n").Replace("|", "**");

            return new HtmlEncodedString(text);
        }

        public IHtmlEncodedString TranslateSpecialCharsToReddit(string value)
        {
            var text = value;

            text = text.Replace("\\r", "\n").Replace("|", "**");

            return new HtmlEncodedString(text);
        }
    }
}
