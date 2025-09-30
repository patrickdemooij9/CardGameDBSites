namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardReaderOuputItem : Dictionary<string, object>
    {
        public int? Id
        {
            get
            {
                if (ContainsKey("Id") && int.TryParse(this["Id"]?.ToString(), out int idValue))
                {
                    return idValue;
                }
                return null;
            }
        }

        public int? ParentId
        {
            get
            {
                if (ContainsKey("ParentId") && int.TryParse(this["ParentId"]?.ToString(), out int parentIdValue))
                {
                    return parentIdValue;
                }
                return null;
            }
        }

        public int? VariantTypeId 
        {
            get
            {
                if (ContainsKey("VariantTypeId") && int.TryParse(this["VariantTypeId"]?.ToString(), out int variantTypeIdValue))
                {
                    return variantTypeIdValue;
                }
                return null;
            }
        }

        public string Name
        {
            get => this["Name"]?.ToString();
        }

        public string Image
        {
            get => this["image"]?.ToString()!;
        }
        public string ImageBase64
        {
            get => this["image_base64"]?.ToString()!;
        }
    }
}
