using SkytearHorde.Entities.Models.Business;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class RandomizeService
    {
        private static readonly Random _random = new();

        private readonly CardService _cardService;

        public RandomizeService(CardService cardService)
        {
            _cardService = cardService;
        }

        public string RandomizeScenario(RandomizeRequest request)
        {
            var cards = new List<Card>();
            foreach (var set in request.SetIds)
            {
                cards.AddRange(_cardService.GetAllBySet(set));
            }

            if (request.Requirements.Length > 0)
            {
                cards = cards.Where(it => request.Requirements.All(r => r.IsValid([it]))).ToList();
            }

            string[] options;
            // Check if we are searching for a distinct attribute or card
            if (!string.IsNullOrWhiteSpace(request.ReturnDistinctAttribute))
            {
                options = cards.Select(it => it.GetMultipleCardAttributeValue(request.ReturnDistinctAttribute)?.FirstOrDefault()).Distinct().WhereNotNull().ToArray();
            }
            else
            {
                options = cards.Select(it => it.DisplayName).ToArray();
            }

            if (options.Length == 0)
            {
                return string.Empty;
            }

            return options[_random.Next(options.Length)];
        }
    }
}
