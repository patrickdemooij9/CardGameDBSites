namespace SkytearHorde.Entities.Models.Business
{
    public class CardAttribute
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool HideFromDetail { get; set; }
        public bool HideFromDiscord { get; set; }
        public bool HideFromTooltip { get; set; }
        public bool IsDiscordInline { get; set; }
        public bool IsMultiValue { get; set; }
        public bool SeparateDetailArea { get; set; }

        public CardAttribute()
        {
            
        }

        public CardAttribute(Generated.CardAttribute attribute)
        {
            Name = attribute.Name;
            DisplayName = attribute.DisplayName!;
            HideFromDetail = attribute.HideFromDetail;
            HideFromDiscord = attribute.HideFromDiscord;
            HideFromTooltip = attribute.HideFromTooltip;
            IsDiscordInline = attribute.IsDiscordInline;
            IsMultiValue = attribute.IsMultiValue;
            SeparateDetailArea = attribute.SeparateDetailArea;
        }
    }
}
