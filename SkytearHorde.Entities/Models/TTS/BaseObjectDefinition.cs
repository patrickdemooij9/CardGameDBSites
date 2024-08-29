using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.TTS
{
    [JsonDerivedType(typeof(BagDefinition))]
    [JsonDerivedType(typeof(CardDefinition))]
    [JsonDerivedType(typeof(CustomDeckDefinition))]
    public abstract class BaseObjectDefinition
    {
        public abstract string Name { get; }
        public string Nickname { get; set; }
        public Transform Transform { get; set; }
    }
}
