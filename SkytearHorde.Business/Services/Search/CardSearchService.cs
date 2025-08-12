using Examine;
using Examine.Search;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchService : ICardSearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly CardSearchFieldsFinder _cardSearchFieldsFinder;

        public CardSearchService(IExamineManager examineManager, CardSearchFieldsFinder cardSearchFieldsFinder)
        {
            _examineManager = examineManager;
            _cardSearchFieldsFinder = cardSearchFieldsFinder;
        }

        public int[] Search(CardSearchQuery query, out int totalItems)
        {
            if (!_examineManager.TryGetIndex("CardIndex", out var index))
            {
                totalItems = 0;
                return Array.Empty<int>();
            }

            var searcher = index.Searcher
                .CreateQuery()
                .NativeQuery($"+__IndexType:content");

            foreach(var filterClause in query.FilterClauses)
            {
                var filters = filterClause.Filters.Where(it => it.Values.Length > 0).ToArray();
                if (filters.Length == 0)
                {
                    continue;
                }

                GetQuery(searcher, filterClause.ClauseType).Group(GetFiltersOperation(filters));

                /*foreach (var filter in filters)
                {
                    var actualValue = filter.Values.Select(it => it.Replace(" ", "")).ToArray();
                    switch (filter.Mode)
                    {
                        case CardSearchFilterMode.Include:
                            searcher.And().GroupedOr([$"CustomField.{filter.Alias}"], actualValue);
                            break;
                        case CardSearchFilterMode.Exclude:
                            searcher.And().GroupedNot([$"CustomField.{filter.Alias}"], actualValue);
                            break;
                    }
                }*/

                //searcher.Not().NativeQuery("(CustomField.Type:seeker OR CustomField.Type:agent)");
                //searcher.And().Group(GetFiltersOperation(filters));
                //searcher.And().GroupedOr([$"CustomField.{filter.Alias}"], actualValue)
                //searcher.And(GetFiltersOperation(filters));
            }

            searcher.And().Field("siteId", query.SiteId.ToString());
            if (query.SetId.HasValue)
            {
                searcher.And().Field("CustomField.SetId", query.SetId.Value.ToString());
            }
            if (query.VariantTypeId.HasValue)
            {
                searcher.And().Field("VariantType", query.VariantTypeId.Value.ToString());
            }
            if (!query.IncludeHideFromDeck)
            {
                searcher.And().Field("DecksOnly", 1.ToString());
            }

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var splitQuery = query.Query.Replace('-', ' ').Split(' ');
                var searchFields = new List<string> { "name" };
                searchFields.AddRange(_cardSearchFieldsFinder.GetGeneralFieldsToSearch().Select(it => $"CustomField.{it}"));
                foreach (var queryItem in splitQuery.Where(it => !string.IsNullOrWhiteSpace(it)))
                {
                    searcher.And().GroupedOr(searchFields, queryItem.MultipleCharacterWildcard());
                }
            }
            else
            {
                if (query.OrderBy.Count == 0)
                {
                    searcher.OrderBy(new SortableField("sortOrder", SortType.Int));
                }
                else
                {
                    foreach (var orderBy in query.OrderBy)
                    {
                        if (orderBy.IsDescending)
                        {
                            searcher.OrderByDescending(new SortableField(orderBy.Field, SortType.Int));
                        }
                        else
                        {
                            searcher.OrderBy(new SortableField(orderBy.Field, SortType.String));
                        }
                    }
                }
            }

            var results = searcher.Execute(new QueryOptions(query.Skip, query.Amount));
            totalItems = (int)results.TotalItemCount;
            var ids = results.Select(it => int.Parse(it.Id));
            return ids.ToArray();

            /*using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var cards = new List<Card>();
            foreach (var id in ids)
            {
                var foundCard = _cardService.GetVariant(id);
                if (foundCard is null) continue;

                cards.Add(foundCard);
            }

            return cards.ToArray();*/
        }

        private IQuery GetQuery(IBooleanOperation operation, CardSearchFilterClauseType clauseType)
        {
            return clauseType switch
            {
                CardSearchFilterClauseType.OR => operation.Or(),
                CardSearchFilterClauseType.NOT => operation.Not(),
                _ => operation.And(),
            };
        }

        /*private string GetQueryForFilters(CardSearchFilterClause[] clauses)
        {
            var stringBuilder = new StringBuilder();
            foreach (var filterClause in clauses)
            {
                var filters = filterClause.Filters.Where(it => it.Values.Length > 0).ToArray();
                if (filters.Length == 0)
                {
                    continue;
                }

                foreach (var filter in filters)
                {
                    var actualValue = filter.Values.Select(it => it.Replace(" ", "")).ToArray();
                    switch (filter.Mode)
                    {
                        case CardSearchFilterMode.Include:
                            stringBuilder.Append($"")
                            searcher.And().GroupedOr([$"CustomField.{filter.Alias}"], actualValue);
                            break;
                        case CardSearchFilterMode.Exclude:
                            searcher.And().GroupedNot([$"CustomField.{filter.Alias}"], actualValue);
                            break;
                    }
                }

                //searcher.Not().NativeQuery("(CustomField.Type:seeker OR CustomField.Type:agent)");
                //searcher.And().Group(GetFiltersOperation(filters));
                //searcher.And().GroupedOr([$"CustomField.{filter.Alias}"], actualValue)
                //searcher.And(GetFiltersOperation(filters));
            }
        }*/

        private Func<INestedQuery, INestedBooleanOperation> GetFiltersOperation(CardSearchFilter[] filters)
        {
            return it =>
            {
                INestedBooleanOperation? nestedBoolean = null;
                foreach (var filter in filters)
                {
                    var query = nestedBoolean is null ? it : nestedBoolean.Or();
                    var actualValue = filter.Values.Select(it => it.Replace(" ", "")).ToArray();
                    switch (filter.Mode)
                    {
                        case CardSearchFilterMode.Contains:
                            nestedBoolean = query.GroupedOr([$"CustomField.{filter.Alias}"], actualValue);
                            break;
                        case CardSearchFilterMode.Higher:
                            foreach (var value in actualValue)
                            {
                                nestedBoolean = query.RangeQuery<int>([$"CustomField.{filter.Alias}"], min: int.Parse(value), null);
                            }
                            break;
                        case CardSearchFilterMode.Lower:
                            foreach (var value in actualValue)
                            {
                                nestedBoolean = query.RangeQuery<int>([$"CustomField.{filter.Alias}"], null, max: int.Parse(value));
                            }
                            break;
                        case CardSearchFilterMode.Range:
                            var minValue = int.Parse(actualValue[0]);
                            var maxValue = int.Parse(actualValue[1]);
                            //nestedBoolean = query.ManagedQuery($"CustomField.{filter.Alias}:[{minValue} TO {maxValue}]")
                            nestedBoolean = query.RangeQuery<int>([$"CustomField.{filter.Alias}"], minValue, maxValue);
                            break;
                    }
                }
                return nestedBoolean!;
            };
        }
    }
}
