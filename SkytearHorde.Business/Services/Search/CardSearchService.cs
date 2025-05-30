﻿using Examine;
using Examine.Search;
using SkytearHorde.Entities.Models.Business;
using System.Security.Policy;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchService : ICardSearchService
    {
        private readonly IExamineManager _examineManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CardService _cardService;

        public CardSearchService(IExamineManager examineManager, IUmbracoContextFactory umbracoContextFactory,
            CardService cardService)
        {
            _examineManager = examineManager;
            _umbracoContextFactory = umbracoContextFactory;
            _cardService = cardService;
        }

        public Card[] Search(CardSearchQuery query, out int totalItems)
        {
            if (!_examineManager.TryGetIndex("ExternalIndex", out var index))
            {
                totalItems = 0;
                return Array.Empty<Card>();
            }

            var searcher = index.Searcher
                .CreateQuery()
                .NativeQuery($"+__IndexType:content")
                .And()
                .NodeTypeAlias(Entities.Generated.CardVariant.ModelTypeAlias);

            foreach (var filter in query.CustomFields.Where(it => it.Value.Length > 0))
            {
                searcher.And().GroupedAnd([$"CustomField.{filter.Key}"], filter.Value.Select(it => it.Replace(" ", "")).ToArray());
            }

            searcher.And().Field("siteId", query.SiteId.ToString());
            if (query.SetId.HasValue)
            {
                searcher.And().Field("CustomField.SetId", query.SetId.Value);
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
                foreach (var queryItem in splitQuery.Where(it => !string.IsNullOrWhiteSpace(it)))
                {
                    searcher.And().Field("Name", queryItem.MultipleCharacterWildcard());
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

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var cards = new List<Card>();
            foreach(var id in ids)
            {
                var foundCard = _cardService.GetVariant(id);
                if (foundCard is null) continue;

                cards.Add(foundCard);
            }

            return cards.ToArray();
        }
    }
}
