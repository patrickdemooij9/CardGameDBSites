using DeviceDetectorNET.Class.Device;
using J2N;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using YamlDotNet.Serialization;
using static Umbraco.Extensions.NPocoSqlExtensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.ViewComponents
{
    public class CreateSquadViewComponent : ViewComponent
    {
        private readonly CardService _cardService;
        private readonly ViewRenderHelper _viewRenderHelper;
        private readonly ISiteAccessor _siteAccessor;
        private readonly SettingsService _settingsService;
        private readonly DeckService _deckService;
        private readonly IMemberManager _memberManager;
        private readonly CollectionService _collectionService;

        public CreateSquadViewComponent(CardService cardService,
            ViewRenderHelper viewRenderHelper,
            ISiteAccessor siteAccessor,
            SettingsService settingsService,
            DeckService deckService,
            IMemberManager memberManager,
            CollectionService collectionService)
        {
            _cardService = cardService;
            _viewRenderHelper = viewRenderHelper;
            _siteAccessor = siteAccessor;
            _settingsService = settingsService;
            _deckService = deckService;
            _memberManager = memberManager;
            _collectionService = collectionService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CreateSquad currentPage)
        {
            var currentUser = await _memberManager.GetCurrentMemberAsync();

            Deck? existingDeck = null;
            if (currentUser != null && int.TryParse(Request.Query["id"], out var deckId))
            {
                var deck = _deckService.Get(deckId, DeckStatus.None);
                if (deck != null && deck.CreatedBy != null && deck.CreatedBy == int.Parse(currentUser.Id))
                {
                    existingDeck = deck;
                }
            }

            var squadSettings = (currentPage.SettingType as SquadSettings) ?? _settingsService.GetSquadSettings();
            var cardSettings = _settingsService.GetCardSettings();
            var teamName = existingDeck?.Name;
            if (string.IsNullOrWhiteSpace(teamName))
            {
                var random = new Random();
                var defaultNames = squadSettings.DefaultNames?.ToArray();
                if (defaultNames?.Length > 0)
                {
                    teamName = defaultNames[random.Next(0, defaultNames.Length)];
                }
                else
                {
                    teamName = "My new Strike Team";
                }
            }

            var isLoggedIn = _memberManager.IsLoggedIn();
            var teamRequirements = squadSettings.Restrictions.ToItems<ISquadRequirementConfig>().ToArray();
            var teamModel = new CreateSquadTeamViewModel
            {
                Id = existingDeck?.Id,
                TypeId = squadSettings.TypeID,
                SiteId = _siteAccessor.GetSiteId(),
                Name = teamName,
                Description = existingDeck?.Description ?? "",
                IsLoggedIn = isLoggedIn,
                Requirements = teamRequirements.Select(r => new CreateSquadRequirement(r)).ToArray(),
                PreselectFirstSlot = squadSettings.SlotOnlyMode
            };

            var allCards = _cardService.GetAll().ToArray();

            teamModel.Squads = squadSettings.Squads.ToItems<SquadConfig>().Select((squad) => new CreateSquadViewModel
            {
                Id = squad.SquadId,
                Name = squad.Label,
                Slots = squad.Slots.ToItems<SquadSlotConfig>().Select((slot, index) => new CreateSquadSlotViewModel
                {
                    Id = index,
                    CardGroups = GetGroupingBySlot(slot,
                        existingDeck?.Cards.Where(it => it.GroupId.Equals(squad.SquadId) && it.SlotId.Equals(index)).ToArray() ?? [],
                        allCards.ToDictionary(it => it.BaseId, it => it)),
                    Label = slot.Label,
                    Requirements = slot.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new CreateSquadRequirement(r)).ToArray(),
                    MinCards = slot.MinCards,
                    MaxCards = slot.MaxCards,
                    DisplaySize = slot.DisplaySize.IfNullOrWhiteSpace("Medium"),
                    DisableRemoval = slot.DisableRemoval,
                    NumberMode = slot.NumberMode,
                    ShowIfTargetSlotIsFilled = slot.ShowIfTargetSlotIsFilled == 0 ? null : slot.ShowIfTargetSlotIsFilled - 1,
                    AdditionalFilterRequirements = slot.AdditionalRequirementFilters.ToItems<ISquadRequirementConfig>().Select(r => new CreateSquadRequirement(r)).ToArray()
                }).ToArray(),
                Requirements = squad.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new CreateSquadRequirement(r)).ToArray()
            }).ToArray();

            teamModel.HasDynamicSlot = teamModel.Squads.Any(it => it.Slots.Any(s => s.ShowIfTargetSlotIsFilled != null));

            var overwriteAmount = squadSettings.OverwriteAmount > 0;

            var allCharacters = new List<CreateSquadCharacterViewModel>();
            foreach (var character in allCards.Where(it => !it.HideFromDecks))
            {
                var abilities = new List<CreateSquadAbilityViewModel>();
                foreach (var ability in character.Attributes)
                {
                    var value = ability.Value.GetValues();
                    var displayValue = _viewRenderHelper.RenderView($"~/Views/Partials/cardAbilities/{ability.Value.GetType().Name}.cshtml", new CardDetailDisplayViewModel() { AbilityValue = ability.Value }).ToString();

                    if (ability.Key.Name.Equals("Amount") && overwriteAmount)
                    {
                        value = [squadSettings.OverwriteAmount.ToString()];
                    }

                    abilities.Add(new CreateSquadAbilityViewModel
                    {
                        DisplayName = ability.Key.Name,
                        Type = ability.Key.Name,
                        Values = value,
                        DisplayValue = displayValue
                    });
                }

                var images = new List<CreateSquadCharacterImageViewModel>();
                if (character.Image != null)
                {
                    images.Add(new CreateSquadCharacterImageViewModel
                    {
                        ImageUrl = character.Image?.GetCropUrl(width: 600),
                        Label = "Front",
                        IsPrimaryImage = true
                    });
                }
                if (character.BackImage != null)
                {
                    images.Add(new CreateSquadCharacterImageViewModel
                    {
                        ImageUrl = character.BackImage?.GetCropUrl(width: 600),
                        Label = "Back",
                    });
                }
                if (character.Sections.Length > 0)
                {
                    foreach (var section in character.Sections)
                    {
                        var sectionImages = section.Images?.ToArray() ?? Array.Empty<MediaWithCrops>();
                        for (var i = 0; i < sectionImages.Length; i++)
                        {
                            var sectionImage = sectionImages[i];
                            images.Add(new CreateSquadCharacterImageViewModel
                            {
                                ImageUrl = sectionImage.GetCropUrl(width: 600),
                                Label = $"{section.Title} {(i != 0 ? i + 1 : "")}"
                            });
                        }
                    }
                }

                allCharacters.Add(new CreateSquadCharacterViewModel
                {
                    Id = character.BaseId,
                    Name = character.DisplayName,
                    IconUrls = GetIconUrls(character, squadSettings),
                    Abilities = abilities.ToArray(),
                    Images = images.ToArray(),
                    TeamRequirements = character.TeamRequirements.Select(r => new CreateSquadRequirement(r)).ToArray(),
                    SquadRequirements = character.SquadRequirements.Select(r => new CreateSquadRequirement(r)).ToArray(),
                    SlotRequirements = character.SlotTargetRequirements.Select(r => new CreateSquadTargetSlotRequirement()
                    {
                        SlotId = r.TargetSlotId,
                        Requirements = r.Requirements.ToItems<ISquadRequirementConfig>().Select(sr => new CreateSquadRequirement(sr)).ToArray()
                    }).ToArray()
                });
            }
            teamModel.AllCharacters = allCharacters.ToArray();

            teamModel.OwnedCharacters = isLoggedIn ? _collectionService.GetCards().Select(it => it.CardId).Distinct().ToArray() : [];

            var viewModel = new CreateSquadTeamOverviewViewModel(teamModel);

            if (cardSettings != null)
            {
                viewModel.Details.AddRange(cardSettings.Display.ToItems<IPublishedElement>().OfType<CardDetailAbilityDisplay>());
            }

            foreach (var filter in currentPage.Filters.ToItems<OverviewFilter>())
            {
                var abilityName = filter.IsExpansionFilter ? "Set Name" : filter.Ability!.Name;

                var filterModel = new FilterViewModel(filter.DisplayName!, abilityName)
                {
                    IsInline = filter.IsInline
                };
                if (filter.AutoFillValues)
                {
                    foreach (var value in _cardService.GetCardValues(filter.Ability.Name).OrderBy(it => it))
                    {
                        filterModel.Items.Add(new FilterItemViewModel(value, value));
                    }
                }
                else
                {
                    foreach (var item in filter.Items.ToItems<OverviewFilterItem>())
                    {
                        var filterItem = new FilterItemViewModel(item.DisplayName!, item.Value!);
                        if (item.Icon != null)
                        {
                            filterItem.IconUrl = item.Icon.Url();
                        }
                        filterModel.Items.Add(filterItem);
                    }
                }
                viewModel.Filters.Add(filterModel);
            }

            return View("/Views/Partials/components/createSquad.cshtml", viewModel);
        }

        private CreateSquadGroupingViewModel[] GetGroupingBySlot(SquadSlotConfig slot, DeckCard[] deckCards, Dictionary<int, Card> cards)
        {
            var groups = slot.Groupings.ToItems<DeckCardGroup>().ToArray();
            if (groups.Length == 0) // Always have a group to put items in
            {
                return [
                    new CreateSquadGroupingViewModel()
                    {
                        DisplayName = string.Empty,
                        CardIds = deckCards.Select(it => new CreateSquadCardAmountViewModel
                        {
                            Id = it.CardId,
                            Amount = it.Amount,
                            AllowRemoval = true
                        }).ToArray() ?? Array.Empty<CreateSquadCardAmountViewModel>(),
                        Requirements = []
                    }
                    ];
            }

            var returnGroups = new CreateSquadGroupingViewModel[groups.Length];
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var conditions = group.Conditions.ToItems<ISquadRequirementConfig>().ToArray();
                returnGroups[i] = new CreateSquadGroupingViewModel
                {
                    DisplayName = group.Header,
                    CardIds = deckCards.Where(it => conditions.All(c => c.GetRequirement().IsValid([cards[it.CardId]]))).Select(it => new CreateSquadCardAmountViewModel
                    {
                        Id = it.CardId,
                        Amount = it.Amount,
                        AllowRemoval = true
                    }).ToArray() ?? Array.Empty<CreateSquadCardAmountViewModel>(),
                    Requirements = conditions.Select(c => new CreateSquadRequirement(c)).ToArray(),
                    SortBy = "Cost"
                };
            }
            return returnGroups;
        }

        private string[] GetIconUrls(Card card, SquadSettings squadSettings)
        {
            var iconUrls = new List<string>();
            if (squadSettings.SpecialImageRule?.Count > 0)
            {
                var cardArray = new[] { card };
                var conditionsMet = squadSettings.SpecialImageRule.ToItems<CardImage>().Where(it => it.Conditions.ToItems<ISquadRequirementConfig>().All(config => config.GetRequirement().IsValid(cardArray)));

                foreach (var conditionMet in conditionsMet)
                {
                    if (conditionMet.Image != null)
                    {
                        iconUrls.Add(conditionMet.Image.GetCropUrl(width: 50, height: 50));
                    }
                }
            }
            if (iconUrls.Count == 0)
            {
                iconUrls.Add(card.Image?.GetCropUrl("Icon"));
            }
            return iconUrls.ToArray();
        }
    }
}
