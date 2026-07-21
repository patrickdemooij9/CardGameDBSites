using SkytearHorde.Entities.Models.Business;

namespace CardGameDBSites.API.Models.Decks
{
    public class DeckFolderApiModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int DeckCount { get; set; }
        public DateTime CreatedDate { get; set; }

        public DeckFolderApiModel(DeckFolder folder, int deckCount)
        {
            Id = folder.Id;
            Name = folder.Name;
            Description = folder.Description;
            CreatedDate = folder.CreatedDate;
            DeckCount = deckCount;
        }
    }
}
