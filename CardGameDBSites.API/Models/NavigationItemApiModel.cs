namespace CardGameDBSites.API.Models
{
    public class NavigationItemApiModel
    {
        public required string Name { get; set; }
        public required string Url { get; set; }
        public NavigationItemApiModel[] Children { get; set; }

        public NavigationItemApiModel()
        {
            Children = [];
        }
    }
}
