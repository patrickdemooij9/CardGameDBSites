using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SkytearHorde.Business.BackgroundRunners
{
    public class DeckCalculatorTask : RecurringHostedServiceBase
    {
        private readonly DeckService _deckService;
        private readonly DeckViewRepository _deckViewRepository;
        private readonly DeckCalculateScoreRepository _deckCalculateScoreRepository;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IRuntimeState _runtimeState;

        public DeckCalculatorTask(ILogger<DeckCalculatorTask> logger,
            DeckService deckService,
            DeckViewRepository deckViewRepository,
            DeckCalculateScoreRepository deckCalculateScoreRepository,
            ISiteAccessor siteAccessor,
            IRuntimeState runtimeState) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(0))
        {
            _deckService = deckService;
            _deckViewRepository = deckViewRepository;
            _deckCalculateScoreRepository = deckCalculateScoreRepository;
            _siteAccessor = siteAccessor;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            var decksToProcess = _deckCalculateScoreRepository.GetDecksToProcess();

            List<int> decksToRemove = new List<int>();
            foreach (var siteGrouped in decksToProcess.GroupBy(it => it.SiteId))
            {
                _siteAccessor.SetSiteId(siteGrouped.Key);

                var deckIds = siteGrouped.Select(it => it.DeckId).ToArray();
                var decks = _deckService.Get(deckIds, Entities.Enums.DeckStatus.Published).ToDictionary(it => it.Id, it => it);

                var calculator = new DeckCalculator();
                foreach (var deckId in deckIds)
                {
                    if (!decks.ContainsKey(deckId))
                    {
                        decksToRemove.Add(deckId);
                        continue;
                    }
                    var views = _deckViewRepository.GetLast7Days(deckId);

                    var deck = decks[deckId];
                    var score = calculator.CalculateDeckScore(deck, views);
                    if (score != deck.Score)
                    {
                        _deckService.UpdateScore(deck, score);
                    }
                    _deckCalculateScoreRepository.ScheduleDeckCalculate(deckId, DateTime.UtcNow.AddDays(1));
                }
            }
            foreach(var deckId in decksToRemove)
            {
                _deckCalculateScoreRepository.RemoveEntry(deckId);
            }

            return Task.CompletedTask;
        }
    }
}
