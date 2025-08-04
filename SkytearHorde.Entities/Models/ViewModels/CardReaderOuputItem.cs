namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardReaderOuputItem : Dictionary<string, object>
    {
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
