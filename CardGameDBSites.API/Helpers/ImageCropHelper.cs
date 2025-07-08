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
        public static ImageCropsApiModel ToApiModels(MediaWithCrops mediaItem, params string[] crops)
        {
            var result = new ImageCropsApiModel { Url = $"https://aidalon-db.com" + mediaItem.Url(mode: UrlMode.Relative) };
            foreach (var crop in crops)
            {
                result.Crops.Add(new ImageCropApiModel
                {
                    CropAlias = crop,
                    Url = $"https://aidalon-db.com" + mediaItem.GetCropUrl(crop, UrlMode.Relative)!
                });
            }
            return result;
        }
    }
}
