using SkytearHorde.Business.Keywords;

namespace SkytearHorde.Tests.HelperTests
{
    [TestFixture]
    public class KeywordDetectorTests
    {
        private static readonly KeywordDetector Detector = new(new KeywordDetectionConfig
        {
            Keywords = ["Ambush", "Bounty", "Coordinate", "Grit", "Hidden", "Overwhelm", "Plot", "Raid", "Restore", "Saboteur", "Sentinel", "Shielded", "Smuggle"],
            TriggeredAbilities = ["When Played", "When Defeated", "On Attack", "When Attack Ends", "Epic Action", "Action"],
            IgnoredPhrases = ["Bounty Hunter"]
        });

        private static IReadOnlyCollection<DetectedKeyword> Detect(string text, string name = "Test Card") => Detector.Detect(name, [text]);

        [TestCase("Sentinel", "Sentinel")]
        [TestCase("|Raid 2| (This unit gets +2/+0 while attacking.)", "Raid")]
        [TestCase("Raid1", "Raid")]
        [TestCase("SENTINEL (Enemy units in this arena must attack a Sentinel when they attack you.)", "Sentinel")]
        [TestCase("Smuggle [6 [command]]", "Smuggle")]
        [TestCase("Sentinel\\rShielded", "Shielded")]
        [TestCase("|When Defeated:| Draw a card.", "When Defeated")]
        [TestCase("|On Attack:| Create a Beast token and ready it.", "On Attack")]
        [TestCase("When Played/On Attack: Exhaust an enemy unit.", "On Attack")]
        [TestCase("Action [[tap]]: Create a Credit token.", "Action")]
        [TestCase("When Played: Give an enemy unit -3/-0 for this phase.  Smuggle [4, [cunning]]", "Smuggle")]
        public void Detect_KeywordOrTriggerLeadsParagraph_ReturnsHas(string text, string keyword)
        {
            Assert.That(Detect(text), Does.Contain(new DetectedKeyword(keyword, KeywordRelation.Has)));
        }

        [TestCase("While the Force is with you, this unit gains Sentinel.", "Sentinel")]
        [TestCase("While you control 3 or more units, this unit gains Sentinel.", "Sentinel")]
        [TestCase("Shielded\rOn Attack: This unit gains Sentinel for this phase.", "Sentinel")]
        [TestCase("This unit gains Raid1 for each exhausted resource you control.", "Raid")]
        [TestCase("While Bunker Defender is damaged, Bunker Defender gains Sentinel.", "Sentinel")]
        public void Detect_OwnGainBehindCondition_ReturnsConditional(string text, string keyword)
        {
            Assert.That(Detect(text, "Bunker Defender"), Does.Contain(new DetectedKeyword(keyword, KeywordRelation.Conditional)));
        }

        [TestCase("Each other friendly non-leader unit gains Grit.", "Grit")]
        [TestCase("Attached unit gains Overwhelm.", "Overwhelm")]
        [TestCase("When Played: Give a unit Grit for this phase.", "Grit")]
        [TestCase("When Played: You may return a non-leader unit to its owner's hand. It gains Shielded for this phase.", "Shielded")]
        [TestCase("Attached unit gains: \"When Defeated: Deal 1 damage to each enemy base.\"", "When Defeated")]
        [TestCase("Attached unit gains the Rebel trait and: When Defeated: Deal 1 damage to each enemy base.", "When Defeated")]
        [TestCase("Attached unit gains: \"Bounty - Put the top card of your deck into play as a resource.\"", "Bounty")]
        [TestCase("On Attack: The next unit you play this phase gains Hidden.", "Hidden")]
        public void Detect_GainForAnotherUnit_ReturnsGrants(string text, string keyword)
        {
            Assert.That(Detect(text), Does.Contain(new DetectedKeyword(keyword, KeywordRelation.Grants)));
        }

        [TestCase("The next card you play using Plot this phase costs 1 less.", "Plot")]
        [TestCase("If an enemy unit has a Bounty, this unit costs 1 less to play.", "Bounty")]
        [TestCase("When Played: Give a Shield token to each friendly unit with Shielded.", "Shielded")]
        [TestCase("When Played: Each enemy unit loses Hidden for this phase.", "Hidden")]
        [TestCase("Enemy units can't gain Sentinel.", "Sentinel")]
        [TestCase("Sentinel units you control get +1/+1.", "Sentinel")]
        [TestCase("The first unit you play each round that has a \"When Defeated\" ability costs 1 less.", "When Defeated")]
        public void Detect_KeywordOnlyReferenced_ReturnsSynergy(string text, string keyword)
        {
            Assert.That(Detect(text), Does.Contain(new DetectedKeyword(keyword, KeywordRelation.Synergy)));
        }

        [TestCase("Coordinate - Saboteur", "Saboteur")]
        [TestCase("Coordinate - When Played: This unit captures an enemy non-leader unit.", "When Played")]
        public void Detect_AbilityHandedOutByKeyword_ReturnsConditional(string text, string keyword)
        {
            Assert.That(Detect(text), Does.Contain(new DetectedKeyword(keyword, KeywordRelation.Conditional)));
        }

        [Test]
        public void Detect_EpicAction_IsNotAlsoAction()
        {
            var result = Detect("Epic Action: Play a unit from your hand. It costs 1 less.");

            Assert.That(result, Is.EquivalentTo(new[] { new DetectedKeyword("Epic Action", KeywordRelation.Has) }));
        }

        [Test]
        public void Detect_MissingLineBreakAfterKeyword_StillReturnsHas()
        {
            var result = Detect("Overwhelm (When attacking an enemy unit, deal excess damage.) When the regroup phase starts: Give an Advantage token to a unit.");

            Assert.That(result, Does.Contain(new DetectedKeyword("Overwhelm", KeywordRelation.Has)));
        }

        [Test]
        public void Detect_UpgradeNamesItsAttachedUnit_ReturnsGrants()
        {
            var result = Detect("Attach to a non-Vehicle unit.\rIf attached unit is Luke Skywalker, he gains Sentinel.", "Luke's Jedi Lightsaber");

            Assert.That(result, Does.Contain(new DetectedKeyword("Sentinel", KeywordRelation.Grants)));
        }

        [Test]
        public void Detect_LosesOneGainsAnother_SplitsRelations()
        {
            var result = Detect("Sentinel\rWhile this unit is upgraded, he loses Sentinel and gains Saboteur.");

            Assert.That(result, Is.EquivalentTo(new[]
            {
                new DetectedKeyword("Sentinel", KeywordRelation.Has),
                new DetectedKeyword("Sentinel", KeywordRelation.Synergy),
                new DetectedKeyword("Saboteur", KeywordRelation.Conditional)
            }));
        }

        [TestCase("Saboteur (When this unit attacks, ignore Sentinel and defeat the defender's Shields.)", "Sentinel")]
        [TestCase("On Attack: If you control another Bounty Hunter unit, deal 2 damage to a ground unit.", "Bounty")]
        [TestCase("When you draw 1 or more cards during the action phase: Give a Shield token to this unit.", "Action")]
        [TestCase("When Played: Choose a unit to restore to full health.", "Restore")]
        public void Detect_WordIsNotTheKeyword_IgnoresIt(string text, string keyword)
        {
            Assert.That(Detect(text).Select(it => it.Keyword), Has.No.Member(keyword));
        }
    }
}
