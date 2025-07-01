using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class MemberInfoService
    {
        public const string CacheKey = "MemberInfo_";

        private readonly DeckLikeRepository _deckLikeRepository;
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IAppPolicyCache _cache;

        public MemberInfoService(DeckLikeRepository deckLikeRepository, AppCaches appCaches, IMemberManager memberManager, IMemberService memberService)
        {
            _deckLikeRepository = deckLikeRepository;
            _memberManager = memberManager;
            _memberService = memberService;
            _cache = appCaches.RuntimeCache;
        }

        public string? GetName(int memberId)
        {
            return Get(memberId)?.DisplayName;
        }

        public MemberModel? Get(int memberId)
        {
            return _cache.GetCacheItem($"{CacheKey}{memberId}_Name", () =>
            {
                var member = _memberService.GetById(memberId);
                if (member is null) return null;

                return new MemberModel
                {
                    Id = memberId,
                    DisplayName = member.Name!
                };
            });
        }

        public CurrentMemberModel? GetMemberInfo()
        {
            var member = _memberManager.GetCurrentMemberAsync().Result;
            if (member is null) return null;
            var memberId = int.Parse(member.Id);

            return _cache.GetCacheItem($"{CacheKey}{memberId}", () =>
            {
                return new CurrentMemberModel(memberId)
                {
                    DisplayName = member.Name!,
                    LikedDecks = _deckLikeRepository.GetLikedDecks(memberId),
                    IsLoggedIn = true
                };
            }, TimeSpan.FromMinutes(10))!;
        }
    }
}
