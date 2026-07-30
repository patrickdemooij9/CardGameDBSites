using CardGameDBSites.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Helpers
{
    public static class ImageCropHelper
    {
        public const string MediaHost = "https://api.aidalon-db.com";

        /// <summary>Prefixes a relative Umbraco media url with the media host to make it absolute.</summary>
        public static string? ToAbsolute(string? relativeUrl)
            => string.IsNullOrWhiteSpace(relativeUrl) ? null : MediaHost + relativeUrl;

        public static ImageCropsApiModel ToApiModels(MediaWithCrops mediaItem, params string[] crops)
        {
            var result = new ImageCropsApiModel { Url = mediaItem.Url(mode: UrlMode.Absolute) };
            foreach (var crop in crops)
            {
                result.Crops.Add(new ImageCropApiModel
                {
                    CropAlias = crop,
                    Url = mediaItem.GetCropUrl(crop, UrlMode.Absolute)!
                });
            }
            return result;
        }
    }
}
