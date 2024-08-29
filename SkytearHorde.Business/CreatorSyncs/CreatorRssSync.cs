using Microsoft.Extensions.Logging;
using SkytearHorde.Entities.Generated;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace SkytearHorde.Business.CreatorSyncs
{
    public class CreatorRssSync : ICreatorSync
    {
        private readonly ContentCreatorRssSyncConfig _config;
        private readonly ILogger _logger;

        public CreatorRssSync(ContentCreatorRssSyncConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public IEnumerable<CreatorSyncItem> GetAll()
        {
            var items = new List<CreatorSyncItem>();
            try
            {
                using var reader = XmlReader.Create(_config.Url);
                var feed = SyndicationFeed.Load(reader);
                foreach (var item in feed.Items)
                {
                    if (!string.IsNullOrWhiteSpace(_config.Category) && item.Categories.All(it => it.Name != _config.Category)) continue;
                    if (!string.IsNullOrWhiteSpace(_config.TitleContains) && !item.Title.Text.Contains(_config.TitleContains)) continue;

                    var imageUrl = item.ElementExtensions.FirstOrDefault(it => it.OuterName.Equals("content"))?.GetObject<XElement>()?.Attribute("url")?.Value;

                    items.Add(new CreatorSyncItem
                    {
                        Id = item.Id,
                        Title = item.Title.Text,
                        Url = item.Links.First().Uri.ToString(),
                        ImageUrl = imageUrl,
                        PublishedDate = item.PublishDate.UtcDateTime
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Something wrong while trying to sync {_config.Url}");
            }
            return items;
        }
    }
}
