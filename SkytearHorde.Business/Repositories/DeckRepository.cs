using NPoco;
using Org.BouncyCastle.Crypto;
using SkytearHorde.Business.Cache;
using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.Database;
using System.Diagnostics;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories
{
    public class DeckRepository
    {
        public const string CacheKey = "DeckRepository_";

        private readonly IScopeProvider _scopeProvider;
        private readonly Cache.DeckRepositoryCachePolicy _publishedCachePolicy;
        private readonly Cache.DeckRepositoryCachePolicy _unpublishedCachePolicy;

        public DeckRepository(IScopeProvider scopeProvider, IAppPolicyCache cache)
        {
            _scopeProvider = scopeProvider;

            var publishedPolicyOptions = new Cache.RepositoryCachePolicyOptions(() => PerformCount(DeckStatus.Published))
            {
                CacheBaseKey = $"uRepo_{typeof(Deck).Name}_Published_"
            };
            _publishedCachePolicy = new Cache.DeckRepositoryCachePolicy(cache, publishedPolicyOptions);

            var unpublishedPolicyOptions = new Cache.RepositoryCachePolicyOptions(() => PerformCount(DeckStatus.Published))
            {
                CacheBaseKey = $"uRepo_{typeof(Deck).Name}_Unpublished_"
            };
            _unpublishedCachePolicy = new Cache.DeckRepositoryCachePolicy(cache, unpublishedPolicyOptions);
        }

        public int Create(Deck deck)
        {
            GetCachePolicy(deck.IsPublished).Create(deck, DoCreate);

            return deck.Id;
        }

        private void DoCreate(Deck deck)
        {
            using var scope = _scopeProvider.CreateScope();

            var deckModel = new DeckDBModel
            {
                CreatedBy = deck.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                DeckType = deck.TypeId,
                SiteId = deck.SiteId,
                Score = deck.Score
            };
            scope.Database.Insert(deckModel);

            var versionModel = new DeckVersionDBModel
            {
                DeckId = deckModel.Id,
                Name = deck.Name,
                Description = deck.Description,
                CreatedDate = deckModel.CreatedDate,
                Published = deck.IsPublished,
                IsCurrent = true
            };
            scope.Database.Insert(versionModel);

            foreach (var card in deck.Cards)
            {
                scope.Database.Insert(new DeckCardDBModel
                {
                    VersionId = versionModel.Id,
                    CardId = card.CardId,
                    Amount = card.Amount,
                    GroupId = card.GroupId,
                    SlotId = card.SlotId
                });
            }

            scope.Complete();

            deck.Id = deckModel.Id;
        }

        public void Update(Deck deck)
        {
            GetCachePolicy(deck.IsPublished).Update(deck, DoUpdate);
        }

        private void DoUpdate(Deck deck)
        {
            using var scope = _scopeProvider.CreateScope();

            var deckDB = scope.Database.FirstOrDefault<DeckDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckDBModel>()
                .Where<DeckDBModel>(it => it.Id == deck.Id));

            deckDB.Score = deck.Score;

            scope.Database.Update(deckDB);

            var latestVersion = scope.Database.FirstOrDefault<DeckVersionDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckVersionDBModel>()
                .Where<DeckVersionDBModel>(it => it.DeckId == deck.Id && it.Published == deck.IsPublished && it.IsCurrent));

            if (latestVersion != null)
            {
                latestVersion.IsCurrent = false;
                scope.Database.Update(latestVersion);
            }

            var versionModel = new DeckVersionDBModel
            {
                DeckId = deck.Id,
                Name = deck.Name,
                Description = deck.Description,
                CreatedDate = DateTime.UtcNow,
                Published = deck.IsPublished,
                IsCurrent = true
            };
            scope.Database.Insert(versionModel);

            foreach (var card in deck.Cards)
            {
                scope.Database.Insert(new DeckCardDBModel
                {
                    VersionId = versionModel.Id,
                    CardId = card.CardId,
                    Amount = card.Amount,
                    GroupId = card.GroupId,
                    SlotId = card.SlotId
                });
            }

            scope.Complete();
        }

        public void SetScore(int deckId, int score)
        {
            using var scope = _scopeProvider.CreateScope();

            var deckDB = scope.Database.FirstOrDefault<DeckDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckDBModel>()
                .Where<DeckDBModel>(it => it.Id == deckId));

            deckDB.Score = score;

            scope.Database.Update(deckDB);
            scope.Complete();

            ClearCache(deckId);
        }

        public void DeleteDeck(int deckId)
        {
            DoDelete(deckId);
            ClearCache(deckId);
        }

        private void DoDelete(int deckId)
        {
            using var scope = _scopeProvider.CreateScope();

            var deck = scope.Database.FirstOrDefault<DeckDBModel>(scope.SqlContext.Sql()
                .SelectAll()
                .From<DeckDBModel>()
                .Where<DeckDBModel>(it => it.Id == deckId));
            if (deck is null) return;

            deck.IsDeleted = true;
            deck.DeletedDate = DateTime.UtcNow;

            scope.Database.Update(deck);

            scope.Complete();
        }

        public IEnumerable<Deck> GetAll(int siteId, DeckPagedRequest request, out int size)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            request.SiteId = siteId;
            var result = GetPaged(request, out size);
            if (request.Status != DeckStatus.None)
            {
                var isPublished = request.Status == DeckStatus.Published;
                result = result.Where(it => it.IsPublished == isPublished);
            }

            if (request.Cards.Length > 0)
            {
                result = result.Where(it => it.Cards.Any(c => request.Cards.Contains(c.CardId)));
            }

            var items = result.ToArray();
            stopwatch.Stop();

            return items;
        }

        public IEnumerable<Deck> GetAll(int siteId, int userId, DeckStatus status = DeckStatus.Published)
        {
            var result = GetAll(status).Where(it => it.CreatedBy == userId && it.SiteId == siteId);
            if (status != DeckStatus.None)
            {
                result = result.Where(it => it.IsPublished == (status == DeckStatus.Published));
            }
            return result.ToArray();
        }

        private IEnumerable<Deck> GetAll(DeckStatus status, params int[]? ids)
        {
            if (status == DeckStatus.None)
            {
                var decks = new List<Deck>();
                decks.AddRange(_publishedCachePolicy.GetAll(ids, (ids) => DoGetAll(DeckStatus.Published, ids)));
                decks.AddRange(_unpublishedCachePolicy.GetAll(ids, (ids) => DoGetAll(DeckStatus.Saved, ids)));
                return decks;
            }
            else if (status == DeckStatus.Published)
            {
                return _publishedCachePolicy.GetAll(ids, (ids) => DoGetAll(DeckStatus.Published, ids));
            }
            return _unpublishedCachePolicy.GetAll(ids, (ids) => DoGetAll(DeckStatus.Saved, ids));
        }

        private IEnumerable<Deck> GetPaged(DeckPagedRequest request, out int size)
        {
            if (request.Status == DeckStatus.None)
            {
                var decks = new List<Deck>();
                decks.AddRange(_publishedCachePolicy.GetPaged(request, DoGetPaged, (ids) => DoGetAll(DeckStatus.Published, ids), out var publishedTotal));
                decks.AddRange(_unpublishedCachePolicy.GetPaged(request, DoGetPaged, (ids) => DoGetAll(DeckStatus.Saved, ids), out var unpublishedTotal));
                size = publishedTotal + unpublishedTotal;
                return decks;
            }
            else if (request.Status == DeckStatus.Published)
            {
                return _publishedCachePolicy.GetPaged(request, DoGetPaged, (ids) => DoGetAll(DeckStatus.Published, ids), out size);
            }
            return _unpublishedCachePolicy.GetPaged(request, DoGetPaged, (ids) => DoGetAll(DeckStatus.Saved, ids), out size);
        }

        private DeckPagedResult DoGetPaged(DeckPagedRequest request)
        {
            using var scope = _scopeProvider.CreateScope();

            var isPublished = request.Status == DeckStatus.Published;
            var sql = BaseQuery(scope.SqlContext);

            if (request.Cards.Length > 0)
            {
                sql = sql.InnerJoin(scope.SqlContext.Sql()
                        .Select<DeckCardDBModel>(it => it.VersionId)
                        .From<DeckCardDBModel>()
                        .Append($"WHERE [DeckCard].[CardId] in ({string.Join(',', request.Cards)})")
                        .GroupBy<DeckCardDBModel>(it => it.VersionId)
                        .Append($"HAVING COUNT(*) = {request.Cards.Length}"), "dc").On<DeckVersionDBModel, DeckCardDBModel>((left, right) => left.Id == right.VersionId, "dv", "dc");
            }

            if (request.UseUserCollectionId.HasValue)
            {
                var collectionSql = scope.SqlContext.Sql().Select("CardId, SUM(Amount) as 'Total'")
                    .From<CollectionCardDBModel>()
                    .Where<CollectionCardDBModel>(it => it.UserId == request.UseUserCollectionId.Value)
                    .GroupBy<CollectionCardDBModel>(it => it.CardId);
                var deckCardAmounts = scope.SqlContext.Sql("select dcc.VersionId, SUM(dcc.MissingCards) as 'MissingCards' " +
                    "from (" +
                    "select dcc.VersionId, dcc.CardId, (SELECT CASE WHEN (dcc.Amount - ISNULL(Total, 0)) < 0 THEN 0 ELSE (dcc.Amount - ISNULL(Total, 0)) END) as 'MissingCards' " +
                    "from DeckCard dcc " +
                    $"LEFT JOIN ({collectionSql.SQL}) c on dcc.CardId = c.CardId" +
                    ") dcc " +
                    "group by dcc.VersionId", request.UseUserCollectionId.Value);

                sql = sql.Append($"INNER JOIN ({deckCardAmounts.SQL}) cc ON cc.VersionId = dv.Id", request.UseUserCollectionId.Value);
            }

            sql = sql.Where<DeckDBModel>(it => !it.IsDeleted, "d")
                .Where<DeckVersionDBModel>(it => it.Published == isPublished, "dv")
                .Where<DeckDBModel>(it => it.DeckType == request.TypeId && it.SiteId == request.SiteId, "d");

            sql = request.OrderBy switch
            {
                "popular" => sql.OrderByDescending("Score"),
                "collection" => sql.OrderBy("MissingCards"),
                _ => sql.OrderByDescending("d.CreatedDate"),
            };
            var test = sql.ToString();
            var result = scope.Database.Page<DeckFetchModel>(request.Page, request.Take, sql);
            var deckCards = new List<DeckCardDBModel>();

            foreach (var deckGroup in result.Items.InGroupsOf(2000))
            {
                var group = deckGroup.ToArray();

                deckCards.AddRange(scope.Database.Fetch<DeckCardDBModel>(scope.SqlContext.Sql().SelectAll()
                    .From<DeckCardDBModel>()
                    .WhereIn<DeckCardDBModel>(it => it.VersionId, group.Select(it => it.LatestVersionId))));
            }

            var groupedDeckCards = deckCards.GroupBy(it => it.VersionId).ToDictionary(it => it.Key, it => it);

            var decks = new List<Deck>();
            foreach (var deck in result.Items)
            {
                decks.Add(ToModel(deck, groupedDeckCards.GetValue(deck.LatestVersionId)?.ToArray() ?? Array.Empty<DeckCardDBModel>()));
            }
            return new DeckPagedResult
            {
                Decks = [.. decks],
                Total = (int)result.TotalItems
            };
        }

        private IEnumerable<Deck> DoGetAll(DeckStatus status, params int[]? ids)
        {
            using var scope = _scopeProvider.CreateScope();

            var isPublished = status == DeckStatus.Published;
            var sql = BaseQuery(scope.SqlContext)
                .Where<DeckDBModel>(it => !it.IsDeleted, "d")
                .Where<DeckVersionDBModel>(it => it.Published == isPublished, "dv");

            if (ids?.Length > 0)
            {
                sql = sql.Where<DeckDBModel>(it => ids.Contains(it.Id), "d");
            }

            var deckCards = new List<DeckCardDBModel>();
            var decksDb = scope.Database.Fetch<DeckFetchModel>(sql.OrderByDescending("d.CreatedDate"));

            if (ids?.Length > 0)
            {
                foreach (var deckGroup in decksDb.InGroupsOf(2000))
                {
                    var group = deckGroup.ToArray();

                    deckCards.AddRange(scope.Database.Fetch<DeckCardDBModel>(scope.SqlContext.Sql().SelectAll()
                        .From<DeckCardDBModel>()
                        .WhereIn<DeckCardDBModel>(it => it.VersionId, group.Select(it => it.LatestVersionId))));
                }
            }
            else //Get All
            {
                deckCards.AddRange(scope.Database.Fetch<DeckCardDBModel>(scope.SqlContext.Sql()
                    .Select<DeckCardDBModel>()
                    .From<DeckCardDBModel>()
                    .LeftJoin<DeckVersionDBModel>().On<DeckCardDBModel, DeckVersionDBModel>((left, right) => left.VersionId == right.Id)
                    .Where<DeckVersionDBModel>(it => it.IsCurrent)));
            }

            var groupedDeckCards = deckCards.GroupBy(it => it.VersionId).ToDictionary(it => it.Key, it => it);

            var decks = new List<Deck>();
            foreach (var deck in decksDb)
            {
                decks.Add(ToModel(deck, groupedDeckCards.GetValue(deck.LatestVersionId)?.ToArray() ?? Array.Empty<DeckCardDBModel>()));
            }
            return decks.ToArray();
        }

        private int PerformCount(DeckStatus deckStatus)
        {
            using var scope = _scopeProvider.CreateScope();

            var isPublished = deckStatus == DeckStatus.Published;
            return scope.Database.First<int>(scope.SqlContext.Sql()
                .SelectCount()
                .From<DeckDBModel>()
                .LeftJoin<DeckVersionDBModel>().On<DeckDBModel, DeckVersionDBModel>((left, right) => left.Id == right.DeckId && right.IsCurrent)
                .Where<DeckDBModel>(it => !it.IsDeleted)
                .Where<DeckVersionDBModel>(it => it.Published == isPublished));
        }

        private Cache.IRepositoryCachePolicy<Deck, int> GetCachePolicy(bool isPublished)
        {
            return isPublished ? _publishedCachePolicy : _unpublishedCachePolicy;
        }

        public IEnumerable<Deck> Get(DeckStatus status = DeckStatus.Published, params int[] ids)
        {
            var result = GetAll(status, ids);
            if (status != DeckStatus.None)
            {
                var isPublished = status == DeckStatus.Published;
                result = result.Where(it => it.IsPublished == isPublished);
            }
            return result.ToArray();
        }

        public IEnumerable<Deck> GetHottest(int siteId, int typeId, int amount)
        {
            using var scope = _scopeProvider.CreateScope();

            var deckIds = scope.Database.Fetch<int>("select top(@0) d.Id, MAX(d.Score) as 'Score'" +
                " from Deck d" +
                " join DeckVersion dve on d.Id = dve.DeckId and dve.IsCurrent = 1 and dve.Published = 1" +
                " where d.SiteId = @1" +
                " and d.DeckType = @2" +
                " and d.IsDeleted = 0" +
                " group by d.Id" +
                " order by 'Score' desc;", amount, siteId, typeId);

            return Get(DeckStatus.Published, deckIds.ToArray());
        }

        public Dictionary<int, bool> Exists(IEnumerable<int> ids)
        {
            var requestedIds = ids.ToArray();
            using var scope = _scopeProvider.CreateScope();

            var existingIds = scope.Database.Fetch<int>(scope.SqlContext.Sql()
                .Select<DeckDBModel>(it => it.Id)
                .From<DeckDBModel>()
                .WhereIn<DeckDBModel>(it => it.Id, requestedIds));

            return requestedIds.ToDictionary(it => it, existingIds.Contains);
        }

        public void ClearCache(int id)
        {
            _publishedCachePolicy.ClearCache(id);
            _unpublishedCachePolicy.ClearCache(id);
        }

        private Deck ToModel(DeckFetchModel deck, DeckCardDBModel[] cards)
        {
            return new Deck(deck.Id, deck.Name)
            {
                Description = deck.Description,
                CreatedDate = deck.CreatedDate,
                UpdatedDate = deck.UpdatedDate,
                CreatedBy = deck.CreatedBy,
                SiteId = deck.SiteId,
                TypeId = deck.DeckType,
                AmountOfLikes = deck.AmountOfLikes,
                IsPublished = deck.Published,
                Score = deck.Score,
                Cards = cards.Select(it => new DeckCard(it.CardId, it.GroupId, it.SlotId, it.Amount)).ToList()
            };
        }

        private Sql<ISqlContext> BaseQuery(ISqlContext sqlContext)
        {
            return sqlContext.Sql()
                .Select("d.Id, dv.Id as LatestVersionId, dv.Name, dv.Description, d.CreatedDate, dv.CreatedDate as UpdatedDate, d.CreatedBy, dv.Published, d.SiteId, d.DeckType, d.IsDeleted, d.Score, (SELECT COUNT(*) FROM DeckLike WHERE DeckId = d.Id) as 'AmountOfLikes'")
                .From<DeckDBModel>("d")
                .LeftJoin<DeckVersionDBModel>("dv").On<DeckDBModel, DeckVersionDBModel>((left, right) => left.Id == right.DeckId && right.IsCurrent, "d", "dv");
        }
    }
}