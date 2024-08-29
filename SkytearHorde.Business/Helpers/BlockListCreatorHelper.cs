using Newtonsoft.Json;
using System.Reflection;
using Umbraco.Cms.Core;

namespace SkytearHorde.Business.Helpers
{
    public class BlockListCreatorHelper
    {
        /// <summary>
        /// Creates the Block List JSON from the passed in collection of items. Each item needs to have the <see cref="JsonPropertyAttribute"/> that corresponds to the property alias of the element doc type it is based on
        /// </summary>
        /// <typeparam name="T">The type of your item</typeparam>
        /// <param name="items">The collection of items to create</param>
        /// <param name="contentTypeKey">The GUID for the element type (tip: you can look this up in God Mode!)</param>
        /// <param name="settingsData">Optional settings data - defaults to empty settings</param>
        /// <returns>JSON suitable for Block List</returns>
        /// <exception cref="ArgumentNullException">Raised if items are null</exception>
        public static string GetBlockListJsonFor(List<Dictionary<string, string>> contentData, Guid contentTypeKey, List<Dictionary<string, string>> settingsData = null)
        {
            Blocklist blocklistNew = new Blocklist();  //initialize our new empty model to mimic proper JSON structure
            var dictionaryUdi = new List<Dictionary<string, string>>(); // this contains content UDIs

            foreach (var item in contentData)
            {
                if (!item.ContainsKey("contentTypeKey"))
                {
                    item.Add("contentTypeKey", contentTypeKey.ToString());
                }
                if (!item.ContainsKey("udi"))
                {
                    var udi = new GuidUdi("element", Guid.NewGuid()).ToString();

                    item.Add("udi", udi.ToString());
                    dictionaryUdi.Add(new Dictionary<string, string> { { "contentUdi", udi } });
                }
                else
                {
                    dictionaryUdi.Add(new Dictionary<string, string> { { "contentUdi", item["udi"] } });
                }
            }

            blocklistNew.Layout = new BlockListUdi(dictionaryUdi);
            blocklistNew.ContentData = contentData;
            blocklistNew.SettingsData = settingsData ?? new List<Dictionary<string, string>>();

            return JsonConvert.SerializeObject(blocklistNew);
        }

        public class Blocklist //this class is to mock the correct JSON structure when the object is serialized
        {
            [JsonProperty("layout")]
            public BlockListUdi Layout { get; set; }

            [JsonProperty("contentData")]
            public List<Dictionary<string, string>> ContentData { get; set; }

            [JsonProperty("settingsData")]
            public List<Dictionary<string, string>> SettingsData { get; set; }
        }

        public class BlockListUdi //this is a subclass which corresponds to the "Umbraco.BlockList" section in JSON
        {
            [JsonProperty("Umbraco.BlockList")]  //we mock the Umbraco.BlockList name with JsonPropertyAttribute to match the requested JSON structure
            public List<Dictionary<string, string>> ContentUdi { get; set; }

            public BlockListUdi(List<Dictionary<string, string>> items)
            {
                this.ContentUdi = items;
            }
        }
    }

    // Below is an example class that represents a Block List item. It's for a block list of Location items. 
    // The [JsonProperty] for each item should correspond to your property alias of the corresponding element you are creating.

    public class Location
    {
        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }
    }
}
