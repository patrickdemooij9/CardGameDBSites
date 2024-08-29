namespace SkytearHorde.Entities.Models.TTS
{
    public class BagDefinition : BaseObjectDefinition
    {
        public override string Name => "Bag";

        public BaseObjectDefinition[] ContainedObjects { get; set; }
    }
}
