namespace SkytearHorde.Entities.Generated
{
    public partial class SquadSettings
    {
        ///<summary>
        /// Description
        ///</summary>
        public virtual string? Description => this.GetProperty("description")?.GetValue() as string;

        ///<summary>
        /// Image
        ///</summary>
        public virtual global::Umbraco.Cms.Core.Models.MediaWithCrops? Image =>
            this.GetProperty("image")?.GetValue() as global::Umbraco.Cms.Core.Models.MediaWithCrops;
    }
}
