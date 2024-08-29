using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.Business.Exports
{
    public interface IDeckExportType : IPublishedElement
    {
        string DisplayName { get; }
        string IconName { get; }
        bool IsCopyToClipboard { get; }

        string GetIdentifier();
        string GetUrl();

        string GetMimeType();
        string GetFileName(Deck deck);
    }
}
