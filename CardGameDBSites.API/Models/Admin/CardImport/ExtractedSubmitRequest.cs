namespace CardGameDBSites.API.Models.Admin.CardImport
{
    public class ExtractedSubmitRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
        public string? BackImageBase64 { get; set; }
        public string? SourceUrl { get; set; }
        public Dictionary<string, string> ExtractedData { get; set; } = [];
    }
}
