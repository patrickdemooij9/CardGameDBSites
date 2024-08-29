using SkytearHorde.Entities.Interfaces;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Cache
{
	public class DefaultRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
	where TEntity : class, IEntity
	{
		private static readonly TEntity[] _emptyEntities = []; // const

		protected readonly IAppPolicyCache _cache;
		protected readonly RepositoryCachePolicyOptions _options;

		protected readonly string _entityTypeCacheKey;

		public DefaultRepositoryCachePolicy(IAppPolicyCache cache, RepositoryCachePolicyOptions options)
		{
			_cache = cache;
			_options = options ?? throw new ArgumentNullException(nameof(options));

			_entityTypeCacheKey = options.CacheBaseKey.IfNullOrWhiteSpace($"uRepo_{typeof(TEntity).Name}_");
		}

		/// <inheritdoc />
		public void Create(TEntity entity, Action<TEntity> persistNew)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			try
			{
				persistNew(entity);

                // just to be safe, we cannot cache an item without an identity
                _cache.Insert(GetEntityCacheKey(entity.Id), () => entity, TimeSpan.FromMinutes(5), true);

                // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                ClearBaseCache();
            }
			catch
			{
				// if an exception is thrown we need to remove the entry from cache,
				// this is ONLY a work around because of the way
				// that we cache entities: http://issues.umbraco.org/issue/U4-4259
				_cache.Clear(GetEntityCacheKey(entity.Id));

				// if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
				ClearBaseCache();

				throw;
			}
		}

		/// <inheritdoc />
		public void Update(TEntity entity, Action<TEntity> persistUpdated)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			try
			{
				persistUpdated(entity);

				_cache.Insert(GetEntityCacheKey(entity.Id), () => entity, TimeSpan.FromMinutes(5), true);

				// if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
				ClearBaseCache();
			}
			catch
			{
				// if an exception is thrown we need to remove the entry from cache,
				// this is ONLY a work around because of the way
				// that we cache entities: http://issues.umbraco.org/issue/U4-4259
				_cache.Clear(GetEntityCacheKey(entity.Id));

                // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                ClearBaseCache();

                throw;
			}
		}

		/// <inheritdoc />
		public void Delete(TEntity entity, Action<TEntity> persistDeleted)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			try
			{
				persistDeleted(entity);
			}
			finally
			{
				// whatever happens, clear the cache
				var cacheKey = GetEntityCacheKey(entity.Id);
				_cache.Clear(cacheKey);

                // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                ClearBaseCache();
            }
		}

		public void Delete(TId id, Action<TId> persistDeleted)
		{
			try
			{
				persistDeleted(id);
			}
			finally
			{
				// whatever happens, clear the cache
				var cacheKey = GetEntityCacheKey(id);
				_cache.Clear(cacheKey);

                // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                ClearBaseCache();
            }
		}

		/// <inheritdoc />
		public TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
		{
			var cacheKey = GetEntityCacheKey(id);
			TEntity? fromCache = _cache.GetCacheItem<TEntity>(cacheKey);

			// if found in cache then return else fetch and cache
			if (fromCache != null)
			{
				return fromCache;
			}

			TEntity? entity = performGet(id);

			if (entity != null)
			{
				InsertEntity(cacheKey, entity);
			}

			return entity;
		}

		/// <inheritdoc />
		public TEntity? GetCached(TId id)
		{
			var cacheKey = GetEntityCacheKey(id);
			return _cache.GetCacheItem<TEntity>(cacheKey);
		}

		/// <inheritdoc />
		public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll)
		{
			// if found in cache the return else check
			var cacheKey = GetEntityCacheKey(id);
			TEntity? fromCache = _cache.GetCacheItem<TEntity>(cacheKey);
			return fromCache != null || performExists(id);
		}

		/// <inheritdoc />
		public TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
		{
			if (ids?.Length > 0)
			{
				// try to get each entity from the cache
				// if we can find all of them, return
				TEntity[] entities = ids.Select(GetCached).WhereNotNull().ToArray();
				if (ids.Length.Equals(entities.Length))
				{
					return entities; // no need for null checks, we are not caching nulls
				}
			}
			else
			{
				// get everything we have
				TEntity?[] entities = _cache.GetCacheItemsByKeySearch<TEntity>(_entityTypeCacheKey)
					.ToArray(); // no need for null checks, we are not caching nulls

				if (entities.Length > 0)
				{
					// if some of them were in the cache...
					if (_options.GetAllCacheValidateCount)
					{
						// need to validate the count, get the actual count and return if ok
						if (_options.PerformCount is not null)
						{
							var totalCount = _options.PerformCount();
							if (entities.Length == totalCount)
							{
								return entities.WhereNotNull().ToArray();
							}
						}
					}
					else
					{
						// no need to validate, just return what we have and assume it's all there is
						return entities.WhereNotNull().ToArray();
					}
				}
				else if (_options.GetAllCacheAllowZeroCount)
				{
					// if none of them were in the cache
					// and we allow zero count - check for the special (empty) entry
					TEntity[]? empty = _cache.GetCacheItem<TEntity[]>(_entityTypeCacheKey);
					if (empty != null)
					{
						return empty;
					}
				}
			}

			// cache failed, get from repo and cache
			TEntity[]? repoEntities = performGetAll(ids)?
				.WhereNotNull() // exclude nulls!
				.ToArray();

			// note: if empty & allow zero count, will cache a special (empty) entry
			InsertEntities(ids, repoEntities);

			return repoEntities ?? Array.Empty<TEntity>();
		}

		/// <inheritdoc />
		public void ClearAll() => _cache.ClearByKey(_entityTypeCacheKey);

		public virtual void ClearCache(TId id)
		{
			_cache.Clear(GetEntityCacheKey(id));
			ClearBaseCache();
		}

		protected virtual void ClearBaseCache()
		{
            _cache.Clear(_entityTypeCacheKey);
        }

		protected string GetEntityCacheKey(int id) => _entityTypeCacheKey + id;

		protected string GetEntityCacheKey(TId? id)
		{
			if (EqualityComparer<TId>.Default.Equals(id, default))
			{
				return string.Empty;
			}

			if (typeof(TId).IsValueType)
			{
				return _entityTypeCacheKey + id;
			}

			return _entityTypeCacheKey + id?.ToString()?.ToUpperInvariant();
		}

		protected virtual void InsertEntity(string cacheKey, TEntity entity)
			=> _cache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);

		protected virtual void InsertEntities(TId[]? ids, TEntity[]? entities)
		{
			if (ids?.Length == 0 && entities?.Length == 0 && _options.GetAllCacheAllowZeroCount)
			{
				// getting all of them, and finding nothing.
				// if we can cache a zero count, cache an empty array,
				// for as long as the cache is not cleared (no expiration)
				_cache.Insert(_entityTypeCacheKey, () => _emptyEntities);
			}
			else
			{
				if (entities is not null)
				{
					// individually cache each item
					foreach (TEntity entity in entities)
					{
						TEntity capture = entity;
						_cache.Insert(GetEntityCacheKey(entity.Id), () => capture, TimeSpan.FromMinutes(5), true);
					}
				}
			}
		}
	}
}
