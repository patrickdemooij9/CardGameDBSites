using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class MemberInfoService
    {
        public const string CacheKey = "MemberInfo_";

        private readonly DeckLikeRepository _deckLikeRepository;
        private readonly IMemberManager _memberManager;
        private readonly IAppPolicyCache _cache;

        public MemberInfoService(DeckLikeRepository deckLikeRepository, AppCaches appCaches, IMemberManager memberManager)
        {
            _deckLikeRepository = deckLikeRepository;
            _memberManager = memberManager;
            _cache = appCaches.RuntimeCache;
        }

        public MemberModel? GetMemberInfo()
        {
            var member = _memberManager.GetCurrentMemberAsync().Result;
            if (member is null) return null;
            var memberId = int.Parse(member.Id);

            return _cache.GetCacheItem($"{CacheKey}{memberId}", () =>
            {
                return new MemberModel(memberId)
                {
                    LikedDecks = _deckLikeRepository.GetLikedDecks(memberId),
                    IsLoggedIn = true
                };
            }, TimeSpan.FromMinutes(10))!;
        }
    }
}
