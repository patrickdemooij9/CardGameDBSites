namespace CardGameDBSites.API.Models
{
    public class ImageCropsApiModel
    {
        public required string Url { get; set; }
        public List<ImageCropApiModel> Crops { get; } = [];
    }
}
