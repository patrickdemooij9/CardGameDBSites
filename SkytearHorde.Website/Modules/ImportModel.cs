namespace SkytearHorde.Modules
{
    public class ImportModel
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public int? VariantTypeId { get; set; }
        public int? ImageId { get; set; }
        public string Name { get; set; }
        public string SetName { get; set; }
        public bool HideFromDecks { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public ImportModel(int? id, string name, string setName)
        {
            Id = id;
            Name = name;
            SetName = setName;
            Properties = new Dictionary<string, string>();
        }


        public ImportModel(int? id, string name, string setName, Dictionary<string, string> properties)
        {
            Id = id;
            Name = name;
            SetName = setName;
            Properties = properties;
        }
    }
}
