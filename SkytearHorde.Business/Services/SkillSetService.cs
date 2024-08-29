using SkytearHorde.Entities.Models.ResultModels;
using System.Net.Http.Json;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Services
{
    public class SkillSetService
    {
        private readonly HttpClient _httpClient;
        private readonly IAppPolicyCache _cache;

        public SkillSetService(HttpClient httpClient, AppCaches appCaches)
        {
            _httpClient = httpClient;
            _cache = appCaches.RuntimeCache;
        }

        public async Task<string> GetSkillSetData(int skillSetId)
        {
            return await _cache.GetCacheItem($"SkillSetData_{skillSetId}", async () =>
            {
                return await _httpClient.GetStringAsync($"https://www.skilldisplay.eu/api/v1/skillset/{skillSetId}");
            })!;
        }

        public async Task<SkillDisplayOrganizationItem[]> GetOrganizationData(int id, int[] skills)
        {
            var data = (await _cache.GetCacheItem($"SkillSet_GetOrganizationData_{id}", async () =>
            {
                return await _httpClient.GetFromJsonAsync<SkillDisplayOrganizationItem[]>($"https://www.skilldisplay.eu/api/v1/organisation/{id}/listVerifications/json") ?? Array.Empty<SkillDisplayOrganizationItem>();
            }, TimeSpan.FromHours(3))!).ToArray();

            var users = new Dictionary<string, Guid>();
            var items = new List<SkillDisplayOrganizationItem>();
            foreach(var item in data)
            {
                if (!users.ContainsKey(item.User))
                {
                    users[item.User] = Guid.NewGuid();
                }

                item.User = users[item.User].ToString();
                item.LastName = item.LastName[0].ToString();
                if (skills.Contains(item.SkillUid))
                {
                    items.Add(item);
                }
            }
            return items.ToArray();
        }
    }
}
