using Markdig;

namespace SkytearHorde.Business.Helpers
{
    public static class MarkdownHelper
    {
        public static string ToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;

            var pipeline = new MarkdownPipelineBuilder()
                .DisableHtml()
                .UseSoftlineBreakAsHardlineBreak()
                .ConfigureNewLine("/n")
                .UseReferralLinks("nofollow")
                .UseMediaLinks()
                .Build();
            return Markdown.ToHtml(markdown, pipeline).Replace("/n", "</br>");
        }
    }
}
