namespace CardGameDBSites.API.Models.Collection
{
    public class PackPostApiModel
    {
        public int SetId { get; set; }
        public PackItemApiModel[] Items { get; set; }
    }
}
