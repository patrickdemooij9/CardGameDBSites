using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Services
{
    public class MemberSettingsService
    {
        private const int FallbackMissingCopiesThreshold = 3;

        private readonly MemberSettingsRepository _memberSettingsRepository;
        private readonly SettingsService _settingsService;

        public MemberSettingsService(MemberSettingsRepository memberSettingsRepository, SettingsService settingsService)
        {
            _memberSettingsRepository = memberSettingsRepository;
            _settingsService = settingsService;
        }

        public MemberSettings Get(int memberId)
        {
            return _memberSettingsRepository.Get(memberId) ?? new MemberSettings(memberId);
        }

        public void Save(MemberSettings settings)
        {
            _memberSettingsRepository.Save(settings);
        }

        public int GetDefaultMissingCopiesThreshold()
        {
            var siteDefault = _settingsService.GetCollectionSettings().MissingCopiesThreshold;
            return siteDefault > 0 ? siteDefault : FallbackMissingCopiesThreshold;
        }

        public int GetMissingCopiesThreshold(int memberId)
        {
            return _memberSettingsRepository.Get(memberId)?.MissingCopiesThreshold ?? GetDefaultMissingCopiesThreshold();
        }
    }
}
