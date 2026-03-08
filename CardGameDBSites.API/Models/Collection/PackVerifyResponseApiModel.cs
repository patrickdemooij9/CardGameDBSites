namespace CardGameDBSites.API.Models.Collection
{
    public class PackVerifyCardApiModel
    {
        public int BaseId { get; set; }
        public string DisplayName { get; set; }
        public int? VariantTypeId { get; set; }
    }

    public class PackVerifySuccessApiModel
    {
        public int SetId { get; set; }
        public PackVerifyCardApiModel[] Cards { get; set; }
    }

    public class PackVerifyErrorApiModel
    {
        public string ErrorMessage { get; set; }
        public PackPostApiModel PostContent { get; set; }
    }
}
