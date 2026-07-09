# Performance test harness

Tools for validating that backend perf changes actually help. **Default to the in-process
A/B benchmark first** — on this dev setup (shared machine, real SQL Server), k6/HTTP timing
has run-to-run variance of ±25% or more, which swallows most single-method fixes. Reach for
k6 specifically when the fix is about *concurrency* (thread-pool blocking, lock contention)
— those only show up under concurrent load in the first place, so an in-process loop won't
reproduce them; a single manual request always looks fine too.

## Default method: in-process A/B benchmark

Add a throwaway `Development`-only minimal API endpoint directly in `Program.cs` that runs
the *old* and *new* code paths back-to-back, in the same request, against the real
dependencies (real DB, real Umbraco content cache) — no mocks. Running both in the same
process eliminates cross-run noise (JIT warm-up, DB buffer cache state, background load)
almost entirely, since both versions experience identical conditions milliseconds apart.

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/_debug/bench-X", (MyService svc, int? iterations) =>
    {
        var count = iterations is > 0 ? iterations.Value : 100000;

        var swOld = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < count; i++) { /* old code path */ }
        swOld.Stop();

        var swNew = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < count; i++) { /* new code path */ }
        swNew.Stop();

        return Results.Ok(new {
            oldAvgMs = swOld.Elapsed.TotalMilliseconds / count,
            newAvgMs = swNew.Elapsed.TotalMilliseconds / count
        });
    });
}
```

Usage:
1. Hit it once with a small `iterations` value to JIT-warm both code paths.
2. Hit it again with a larger value (start around 20-50 for real DB work, 100k+ for pure
   CPU-only work) for the real numbers.
3. **Repeat 2-3 times.** Trust the result only if the direction is consistent across
   repeats — if the two numbers are close (within ~20%) or the winner flips between runs,
   this informal method isn't rigorous enough to call it; either run more repeats or reach
   for a real tool (BenchmarkDotNet) before making a claim.
4. Delete the endpoint once you have your answer — it's not meant to ship.

If the "old" and "new" logic can't both exist side-by-side in one endpoint (e.g. the fix
changes a class's own field/constructor, like adding a cache field), fall back to two
separate runs — `git stash` the change, rebuild, restart, measure; `git stash pop`,
rebuild, restart, measure again. This reintroduces cross-process noise, so lean harder on
the "repeat and check consistency" step.

**Validated so far with this method:**
- `SettingsService.GetSiteSettings()` caching: ~30µs → ~2.5µs per call (~10-13x, pure CPU,
  100k-iteration loop, separate-process comparison since the fix added a cache field)
- `CardApiController.MapToApiModels` N+1 batching: 2.4-3.9ms → 1.5-1.8ms per 50-card page
  (~1.6-2.2x, real DB work, same-request A/B comparison, consistent across 3 repeats)
- `CardRepository.GetVariant` caching (`CardVariantCachePolicy`): uncached `Map()` averages
  ~0.2-0.27ms/card (11-13ms per 50-card page); cached hits average ~0.0003-0.004ms — 50-700x
  in practice (an idealized dictionary-only simulation suggested ~10,000x, but the real
  `IAppPolicyCache` lookup has its own overhead, which is why validating against the actual
  implementation matters more than a simulated stand-in).
  **Memory concern check**: caching every mapped card for one site would cost ~100MB
  (measured via `GC.GetTotalMemory(true)` before/after populating a `List<Card>` with the
  full catalog — 10,556 cards, ~10KB avg each). Since this runs on a multi-tenant host,
  that's per-site, so uncapped caching risked hundreds of MB to 1GB+ across all sites. Fixed
  with FIFO eviction capped at 3,000 entries (~30MB, shared globally across all sites since
  `CardRepository` is a process-wide singleton and Umbraco content IDs are globally unique).
  Verified the cap holds: fetched 3,100 distinct cards, re-fetched the first (oldest) one,
  and it cost ~0.26ms — back in cache-miss territory, confirming eviction actually fires
  rather than the cache silently growing unbounded.

**Bandwidth/payload-size fixes are even simpler to validate — skip the loop entirely.**
Response compression doesn't change CPU cost, it changes bytes on the wire, and that's
deterministic (no timing noise at all) — just diff `curl -w "%{size_download}"` with and
without an `Accept-Encoding` header:
```
curl -o /dev/null -w "%{size_download}\n" <url>                          # uncompressed
curl -H "Accept-Encoding: gzip" -o /dev/null -w "%{size_download}\n" <url> # compressed
```
Don't use `curl --compressed` for this — it auto-decodes the response before reporting
size, which hides the effect you're trying to measure.
`AddResponseCompression()`/`UseResponseCompression()` in `Program.cs`: a 50-card
`/api/cards/query` response went from 50,552 bytes uncompressed to 4,716 bytes gzip
(90.7% smaller) / 6,159 bytes brotli (87.8% smaller). Note this won't show up as a
latency win in local k6 runs — there's no bandwidth bottleneck on localhost, so the
wall-clock effect only shows up on real (slower/higher-latency) networks in production.

## k6: for concurrency-sensitive fixes only

Use these when the fix changes *how many requests fit through at once* (thread-pool
blocking, lock contention) — effects that genuinely only appear under concurrent load.

### Prerequisites

- [k6](https://k6.io/) installed (`choco install k6` on Windows).
- The site running locally against your dev DB:
  ```
  dotnet run --project SkytearHorde.Website/SkytearHorde.csproj --urls "http://localhost:58529"
  ```
- A `Host` header matching one of the Umbraco domains configured in the DB — the site
  resolves `SiteId` per-request from `SiteSetterMiddleware`/`SiteAccessor` based on the
  request's domain, and requests to a host with no matching domain will 500. Check
  `SELECT domainName FROM umbracoDomain` in your dev DB for valid values. The scripts
  default to `localhost:44344` (maps to the SWU site in the current dev DB backup).

### Scripts

| Script | Endpoint | Auth | What it exercises |
|---|---|---|---|
| `k6/card-search-anonymous.js` | `POST /api/cards/query` | none | General card search — highest-traffic endpoint, no member lookup |
| `k6/card-search-collection-sort.js` | `POST /api/cards/query` (sortBy=collection) | JWT | The member-lookup branch in `CardApiController.Query` |
| `k6/get-current-member.js` | `GET /api/account/GetCurrentMember` | JWT | `MemberInfoService.GetMemberInfo[Async]` directly |
| `k6/settings-site.js` | `GET /api/settings/site` | none | `SettingsService.GetSiteSettings()` in isolation — no search/DB work in the request |

`PAGE_SIZE` (env var, default 20) is also supported on `card-search-anonymous.js` — use a larger
value (e.g. 50) to amplify per-card N+1 fixes in `CardApiController.MapToApiModels`.

Authenticated scripts register a throwaway member via `/api/account/Register` in `setup()`
(recaptcha is unenforced when `RecaptchaSecret` is empty, as in dev) and reuse the token
across all VUs.

### Running a single test

```
k6 run perf-tests/k6/get-current-member.js
```

Useful env vars (all optional): `BASE_URL` (default `http://localhost:58529`),
`HOST_HEADER` (default `localhost:44344`), `VUS` (default varies per script, 50-100).

```
k6 run --env VUS=150 perf-tests/k6/card-search-collection-sort.js
```

### Before/after comparison workflow

1. Save a summary of the **current** code: `k6 run --summary-export=perf-tests/results/after.json perf-tests/k6/get-current-member.js`
2. `git stash` (or checkout the prior commit) to restore the old code, rebuild, restart `dotnet run`.
3. Save the baseline: `k6 run --summary-export=perf-tests/results/before.json perf-tests/k6/get-current-member.js`
4. `git stash pop` (or checkout back), rebuild, restart the site.
5. Compare `before.json`/`after.json` — look at `http_req_duration` (p95/p99), `http_reqs`
   (throughput), and `http_req_failed` (error rate under load).
6. **Re-run step 1 once more with no code changes.** If the "after" number moves as much as
   your before/after delta did, the result is noise — stop trusting it and don't report a win.

`perf-tests/results/` is where exported summaries land — gitignored scratch space, not
committed artifacts.

**Validated so far with this method:**
- Sync-over-async fixes (`MemberInfoService`, `CardApiController.Query`,
  `AccountApiController`): +14-17% throughput, consistent across the concurrency-sensitive
  endpoints — this is the one category where k6 gave a trustworthy signal on the first try.
