using System.Text.Json.Serialization;

namespace CardGameDBSites.API.Models.Decks
{
    [JsonDerivedType(typeof(IconDeckDisplayApiModel))]
    [JsonDerivedType(typeof(SquadDeckDisplayApiModel))]
    public interface IDeckDisplayApiModel
    {
        string Type { get; }
    }
}
