using System.Text.Json.Serialization;

namespace CardGameDBSites.API.Models.Admin.Pricing
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MissingPriceReason
    {
        NoTcgPlayerId,
        InvalidTcgPlayerId,
        NoPriceFound
    }
}
