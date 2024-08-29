namespace SkytearHorde.Business.Exports
{
    public class TTSExportConfig
    {
        public string BackImageUrl { get; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public bool OrderDescending { get; set; }

        public TTSExportConfig(string backImageUrl)
        {
            BackImageUrl = backImageUrl;
        }
    }
}
