using SkytearHorde.Entities.Requirements;
using SkytearHorde.Tests.Utils;

namespace SkytearHorde.Tests.RequirementTests
{
    [TestFixture]
    public class PointsRequirementTests
    {
        [Test]
        public void IsValid_TotalPointsWithinRange_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_TotalPointsBelowMin_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-5" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_TotalPointsAboveMax_ReturnsFalse()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "15" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_TotalPointsExactMin_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "5" } }
                })
            };

            var requirement = new PointsRequirement("Points", 5, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_TotalPointsExactMax_ReturnsTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "10" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoMinConstraint_ReturnsTrueWhenBelowMax()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-100" } }
                })
            };

            var requirement = new PointsRequirement("Points", null, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoMaxConstraint_ReturnsTrueWhenAboveMin()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "100" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_NoConstraints_AlwaysTrue()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-100" } }
                })
            };

            var requirement = new PointsRequirement("Points", null, null);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_EmptyCards_WithConstraints_ReturnsFalse()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new PointsRequirement("Points", 1, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_EmptyCards_NoConstraints_ReturnsTrue()
        {
            var cards = Array.Empty<Entities.Models.Business.Card>();

            var requirement = new PointsRequirement("Points", null, null);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_CardMissingPointsAttribute_TreatedAsZero()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                }),
                CardTestHelper.CreateCard(2) // No points attribute
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True); // 6 + 0 = 6, which is within [0, 10]
        }

        [Test]
        public void IsValid_MultipleCardsWithPoints_SumsCorrectly()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-4" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            // Total: 6 - 3 - 4 = -1, which is < 0
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_IssueExample_AAlone()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_IssueExample_APlusB()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            // Total: 6 - 3 = 3, which is >= 0
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_IssueExample_APlusBPlusC()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3" } }
                }),
                CardTestHelper.CreateCard(3, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-4" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            // Total: 6 - 3 - 4 = -1, which is < 0
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_IssueExample_BAlone()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, null);
            var result = requirement.IsValid(cards);

            // Total: -3, which is < 0
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValid_DecimalPoints_WorksCorrectly()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "6.5" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "-3.2" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            // Total: 6.5 - 3.2 = 3.3, which is within [0, 10]
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValid_InvalidPointsValue_Ignored()
        {
            var cards = new[]
            {
                CardTestHelper.CreateCard(1, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "invalid" } }
                }),
                CardTestHelper.CreateCard(2, new Dictionary<string, string[]>
                {
                    { "Points", new[] { "5" } }
                })
            };

            var requirement = new PointsRequirement("Points", 0, 10);
            var result = requirement.IsValid(cards);

            // Total: 0 (invalid ignored) + 5 = 5, which is within [0, 10]
            Assert.That(result, Is.True);
        }
    }
}
