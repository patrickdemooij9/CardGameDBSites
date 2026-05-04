using Microsoft.Extensions.Logging;
using SkytearHorde.Business.Config;
using System.Net.Http.Json;
using System.Web;

namespace SkytearHorde.Business.Integrations.MeleeGg
{
    /// <summary>
    /// Thin HTTP wrapper around the publicly accessible melee.gg REST API.
    /// Endpoint paths are based on the API surface used by third-party community
    /// tools (e.g. SWUStats). Adjust the constants below if melee.gg changes
    /// their routing.
    /// </summary>
    public class MeleeGgApiClient
    {
        private const int PageSize = 100;

        private readonly HttpClient _httpClient;
        private readonly ILogger<MeleeGgApiClient> _logger;

        public MeleeGgApiClient(HttpClient httpClient, ILogger<MeleeGgApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Returns all completed tournaments for the given game slug that started
        /// after <paramref name="since"/>.
        /// </summary>
        public async Task<List<MeleeGgTournamentSummary>> GetCompletedTournamentsAsync(
            MeleeGgConfig config,
            DateTime since,
            CancellationToken ct = default)
        {
            var results = new List<MeleeGgTournamentSummary>();
            int skip = 0;
            bool hasMore = true;

            while (hasMore)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["game"] = config.GameSlug;
                query["status"] = "completed";
                query["take"] = PageSize.ToString();
                query["skip"] = skip.ToString();

                var url = $"{config.BaseUrl.TrimEnd('/')}/api/v1/tournaments?{query}";

                MeleeGgTournamentListResponse? page = null;
                try
                {
                    page = await _httpClient.GetFromJsonAsync<MeleeGgTournamentListResponse>(url, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch melee.gg tournament list (skip={Skip})", skip);
                    break;
                }

                if (page is null || page.Items.Count == 0)
                    break;

                // Only keep tournaments that started after the cutoff
                var relevant = page.Items.Where(t => t.StartDate >= since).ToList();
                results.AddRange(relevant);

                // If all items on this page are before the cutoff date, stop paginating
                if (relevant.Count < page.Items.Count)
                {
                    hasMore = false;
                }
                else
                {
                    skip += PageSize;
                    hasMore = skip < page.TotalCount;
                }
            }

            return results;
        }

        /// <summary>
        /// Returns the registrations (players + standings) for a specific tournament.
        /// </summary>
        public async Task<List<MeleeGgRegistration>> GetRegistrationsAsync(
            MeleeGgConfig config,
            string tournamentId,
            CancellationToken ct = default)
        {
            var url = $"{config.BaseUrl.TrimEnd('/')}/api/v1/tournaments/{tournamentId}/registrations";

            try
            {
                return await _httpClient.GetFromJsonAsync<List<MeleeGgRegistration>>(url, ct)
                       ?? new List<MeleeGgRegistration>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch melee.gg registrations for tournament {Id}", tournamentId);
                return new List<MeleeGgRegistration>();
            }
        }
    }
}
