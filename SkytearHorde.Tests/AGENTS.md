# Test Writing Agent — Quick Reference

Use this document as the **subagent prompt** when you need to write new .NET unit tests for this project. Pass it as the task description to the `general` subagent.

---

## Project Context

- **Framework:** .NET 8.0 (ASP.NET Core + Umbraco 13.3.0 CMS)
- **Test Framework:** NUnit 4.1.0 + Moq 4.20.70
- **Test Project:** `SkytearHorde.Tests/SkytearHorde.Tests.csproj`
- **References:** `SkytearHorde.Entities` (domain models, requirements) and `SkytearHorde.Business` (services, helpers)

## Directory Layout

```
SkytearHorde.Tests/
├── Utils/                     # Shared test helpers
│   └── CardTestHelper.cs      # Card builder with mocked IAbilityValue
├── RequirementTests/          # Tests for ISquadRequirement implementations
├── HelperTests/               # Tests for static/helper classes
├── ModelTests/                # Tests for domain model logic
├── Usings.cs                  # global using NUnit.Framework;
└── SkytearHorde.Tests.csproj
```

## Build & Run Commands

```bash
dotnet build SkytearHorde.Tests/SkytearHorde.Tests.csproj --no-restore
dotnet test SkytearHorde.Tests/SkytearHorde.Tests.csproj --no-build
```

## CardTestHelper — Core Utility

Always use `CardTestHelper` (at `Utils/CardTestHelper.cs`) for building test `Card` objects:

```csharp
using SkytearHorde.Tests.Utils;

// Card with no attributes
var card = CardTestHelper.CreateCard(1);

// Card with attributes (key = attribute name, value = string[] of values)
var card = CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
{
    { "Faction", new[] { "Fire" } },
    { "Cost", new[] { "3" } }
});

// Card with custom SetName
var card = CardTestHelper.CreateCard(1, "My Set");

// Card with allowed children (for ChildOfSquadRequirement tests)
var card = CardTestHelper.CreateCardWithChildren(1, new[] { 10, 20, 30 }, maxChildren: 5);
```

## Test Class Template

```csharp
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests  // or HelperTests, ModelTests
{
    [TestFixture]
    public class MyClassTests
    {
        [Test]
        public void MethodName_Condition_ExpectedResult()
        {
            // Arrange
            var input = ...;

            // Act
            var result = ...;

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
```

## NUnit Assertion Patterns

```csharp
Assert.That(result, Is.True);
Assert.That(result, Is.False);
Assert.That(result, Is.EqualTo(expected));
Assert.That(result, Is.Null);
Assert.That(result, Is.Not.Null);
Assert.That(result, Is.TypeOf<int>());
Assert.That(result, Is.GreaterThan(5));
Assert.That(result, Is.GreaterThanOrEqualTo(10));
Assert.That(result, Is.LessThan(100));
Assert.That(collection, Has.Length.EqualTo(3));
Assert.That(collection, Has.Count.EqualTo(2));
Assert.That(list[0], Has.Length.EqualTo(5));
Assert.That(stringValue, Does.Contain("substring"));
Assert.That(stringValue, Does.Not.Contain("text"));
Assert.That(stringValue, Is.EqualTo(""));
Assert.That(result, Is.Empty);
```

## Moq Patterns

```csharp
// Mock an interface
var mock = new Mock<ISquadRequirement>();
mock.Setup(r => r.IsValid(It.IsAny<Card[]>())).Returns(true);
mock.Setup(r => r.IsValid(It.IsAny<Card[]>()))
    .Returns<Card[]>(cards => cards.Length > 0);
var obj = mock.Object;

// Mock with verification
mock.Verify(r => r.IsValid(It.IsAny<Card[]>()), Times.Once);

// Mock IAbilityValue (for Card.Attributes dictionary)
var attrMock = new Mock<IAbilityValue>();
attrMock.Setup(a => a.GetValues()).Returns(new[] { "value1", "value2" });
card.Attributes["Key"] = attrMock.Object;
```

## Key Domain Types to Know

| Type | Location | Notes |
|---|---|---|
| `Card` | Entities/Models/Business/Card.cs | `Attributes: Dictionary<string, IAbilityValue>`, `GetMultipleCardAttributeValue(string)` |
| `Deck` | Entities/Models/Business/Deck.cs | `required SiteId`, `required TypeId`, `CalculateCollection(CollectionCardItem[])` |
| `DeckCard` | Entities/Models/Business/DeckCard.cs | Constructor: `new DeckCard(cardId, groupId, slotId, amount)` |
| `ISquadRequirement` | Entities/Interfaces/ | `bool IsValid(Card[] cards)` |
| `IAbilityValue` | Entities/GeneratedExtended/ | `string[] GetValues()` — mock this for attribute tests |
| `ComputedRequirementConfig` | Entities/Models/Business/ | `required` properties for ComputedRequirement |

## Test Naming Convention

`MethodName_ConditionOrInput_ExpectedResult`

Examples:
- `IsValid_AllCardsHaveMatchingValue_ReturnsTrue`
- `IsValid_BelowMin_ReturnsFalse`
- `CalculateDeckScore_LongDescription_Awards20Points`
- `GetMultipleCardAttributeValue_MissingAttribute_ReturnsNull`

## Where to Place New Tests

| What's being tested | Test directory |
|---|---|
| Classes implementing `ISquadRequirement` | `RequirementTests/` |
| Static helpers, formatters, calculators | `HelperTests/` |
| Domain model methods (Card, Deck) | `ModelTests/` |
| Services (require mocking repos) | `ServiceTests/` (create if needed) |
| Controllers | `ControllerTests/` (create if needed) |
