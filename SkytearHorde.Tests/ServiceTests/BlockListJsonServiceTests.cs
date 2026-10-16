using System.Text.Json.Nodes;
using Moq;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SkytearHorde.Tests.ServiceTests
{
    public class BlockListJsonServiceTests
    {
        private static readonly Guid TextAbilityKey = new("A4AC0B27-5103-4E6C-A6E5-111BA1500F26");

        private BlockListJsonService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var contentType = new Mock<IContentType>();
            contentType.Setup(it => it.Alias).Returns("textAbilityValue");
            contentType.Setup(it => it.CompositionPropertyTypes).Returns(
            [
                PropertyType("ability", "Umbraco.MultiNodeTreePicker"),
                PropertyType("value", "Umbraco.TextBox")
            ]);

            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(it => it.Get(TextAbilityKey)).Returns(contentType.Object);
            _service = new BlockListJsonService(contentTypeService.Object);
        }

        [Test]
        public void Serialize_WritesUmbraco17BlockFormat()
        {
            var json = _service.Serialize([new Dictionary<string, string> { ["ability"] = "umb://document/abc", ["value"] = "6" }], TextAbilityKey);

            var root = JsonNode.Parse(json)!;
            var block = root["contentData"]![0]!;
            var key = block["key"]!.GetValue<string>();
            Assert.Multiple(() =>
            {
                Assert.That(block["contentTypeKey"]!.GetValue<string>(), Is.EqualTo(TextAbilityKey.ToString()));
                Assert.That(block["values"]![0]!["alias"]!.GetValue<string>(), Is.EqualTo("ability"));
                Assert.That(block["values"]![0]!["editorAlias"]!.GetValue<string>(), Is.EqualTo("Umbraco.MultiNodeTreePicker"));
                Assert.That(block["values"]![1]!["value"]!.GetValue<string>(), Is.EqualTo("6"));
                Assert.That(root["layout"]!["Umbraco.BlockList"]![0]!["contentKey"]!.GetValue<string>(), Is.EqualTo(key));
                Assert.That(root["expose"]![0]!["contentKey"]!.GetValue<string>(), Is.EqualTo(key));
                Assert.That(root["settingsData"]!.AsArray(), Is.Empty);
            });
        }

        [Test]
        public void Deserialize_RoundTripsItemsInLayoutOrder()
        {
            var first = new Dictionary<string, string> { ["ability"] = "umb://document/first", ["value"] = "1" };
            var second = new Dictionary<string, string> { ["ability"] = "umb://document/second", ["value"] = "2" };

            var items = _service.Deserialize(_service.Serialize([first, second], TextAbilityKey));

            Assert.That(items.Select(it => it["ability"]), Is.EqualTo(new[] { "umb://document/first", "umb://document/second" }));
            Assert.That(items[0][BlockListJsonService.ContentTypeKeyAlias], Is.EqualTo(TextAbilityKey.ToString()));
        }

        [Test]
        public void Serialize_KeepsExistingKeys()
        {
            var json = _service.Serialize([new Dictionary<string, string> { ["value"] = "1" }], TextAbilityKey);
            var item = _service.Deserialize(json).Single();
            item["value"] = "2";

            var updated = _service.Deserialize(_service.Serialize([item], TextAbilityKey)).Single();

            Assert.That(updated[BlockListJsonService.KeyAlias], Is.EqualTo(item[BlockListJsonService.KeyAlias]));
            Assert.That(updated["value"], Is.EqualTo("2"));
        }

        [Test]
        public void Serialize_ThrowsOnUnknownProperty()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _service.Serialize([new Dictionary<string, string> { ["udi"] = "umb://element/abc" }], TextAbilityKey));
        }

        [Test]
        public void Deserialize_ReturnsEmptyForEmptyValue()
        {
            Assert.That(_service.Deserialize(null), Is.Empty);
            Assert.That(_service.Deserialize(""), Is.Empty);
        }

        private static IPropertyType PropertyType(string alias, string editorAlias)
        {
            var propertyType = new Mock<IPropertyType>();
            propertyType.Setup(it => it.Alias).Returns(alias);
            propertyType.Setup(it => it.PropertyEditorAlias).Returns(editorAlias);
            return propertyType.Object;
        }
    }
}
