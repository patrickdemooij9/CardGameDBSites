using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGameDBSites.API.Models
{
    public class SiteSettingsApiModel
    {
        public required string MainColor { get; set; }
        public required string HoverMainColor { get; set; }
        public required string BorderColor { get; set; }
        public required string SiteName { get; set; }
        public required bool ShowLogin { get; set; }
        public string? LoginPageUrl { get; set; }
        public NavigationItemApiModel[] AccountNavigation { get; set; }
        public NavigationItemApiModel[] Navigation { get; set; }
        public required string NavigationLogoUrl { get; set; }
        public required bool TextColorWhite { get; set; }
        public required string FooterText { get; set; }
        public LinkApiModel[] FooterLinks { get; set; }
        public CardSectionApiModel[] CardSections { get; set; }

        public SiteSettingsApiModel()
        {
            AccountNavigation = [];
            Navigation = [];
            FooterLinks = [];
            CardSections = [];
        }
    }
}
