namespace CardGameDBSites.API.Models.Admin
{
    public class SetCardAttributeRequest
    {
        public string Attribute { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<string> CardTypes { get; set; } = [];
        public bool DryRun { get; set; } = true;
    }
}
