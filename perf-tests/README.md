# Performance test harness

k6 load-test scripts for validating that backend changes actually improve (or at least
don't regress) throughput/latency under concurrency. Sync-over-async and thread-pool
issues only show up under concurrent load — a single manual request always looks fine.

## Prerequisites

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

## Scripts

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

## Running a single test

```
k6 run perf-tests/k6/get-current-member.js
```

Useful env vars (all optional): `BASE_URL` (default `http://localhost:58529`),
`HOST_HEADER` (default `localhost:44344`), `VUS` (default varies per script, 50-100).

```
k6 run --env VUS=150 perf-tests/k6/card-search-collection-sort.js
```

## Before/after comparison workflow

1. Save a summary of the **current** code: `k6 run --summary-export=perf-tests/results/after.json perf-tests/k6/get-current-member.js`
2. `git stash` (or checkout the prior commit) to restore the old code, rebuild, restart `dotnet run`.
3. Save the baseline: `k6 run --summary-export=perf-tests/results/before.json perf-tests/k6/get-current-member.js`
4. `git stash pop` (or checkout back), rebuild, restart the site.
5. Compare `before.json`/`after.json` — look at `http_req_duration` (p95/p99), `http_reqs`
   (throughput), and `http_req_failed` (error rate under load). A fix that removes
   thread-pool blocking should show flat/lower p95-p99 latency and higher throughput
   as `VUS` increases, with the gap widening at higher concurrency.

`perf-tests/results/` is where exported summaries land — gitignored scratch space, not
committed artifacts.

## When k6 isn't sensitive enough: in-process micro-benchmarks

k6/HTTP load tests are the right tool for concurrency-sensitive issues (thread-pool
blocking, lock contention) — the fix changes *how many requests fit through at once*.
They are the **wrong** tool for pure per-call CPU savings on a single code path (e.g.
adding a cache around a cheap computation): at real-world concurrency the request's
network/Kestrel/thread-pool-queueing overhead (hundreds of ms) completely swamps a
few-microsecond improvement in one method, on shared hardware, regardless of which
endpoint you pick. Two identical-code k6 runs can easily differ by more than the effect
you're trying to measure — if before/after results are within a run-to-run noise band
(re-run the "after" case once more with no changes; if it moves as much as your before/after
delta, you have your answer), stop trusting the HTTP numbers.

For that case, measure the method directly, in-process, with a throwaway `Development`-only
minimal API endpoint that loops N times and reports average time per call:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/_debug/bench-X", (MyService svc, int? iterations) =>
    {
        var count = iterations is > 0 ? iterations.Value : 100000;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < count; i++) _ = svc.MethodUnderTest();
        sw.Stop();
        return Results.Ok(new { count, avgMicroseconds = sw.Elapsed.TotalMilliseconds * 1000 / count });
    });
}
```

Hit it once with a small `iterations` value to JIT-warm, then again with a large one
(100k+) for the real number. This removes HTTP/network/thread-pool noise entirely and
isolates the method's own cost — delete the endpoint once you have your answer, it's not
meant to ship.

**Even better when the fix does real DB/repository work** (not just CPU, e.g. batching
N+1 calls): a single before/after in-process benchmark can still be noisy request-to-request
against a real DB, exactly like the HTTP case. Run the *old* and *new* code paths
back-to-back inside the same request instead of across separate server restarts — same
warm caches, same DB connection pool, same everything except the code under test:

```csharp
var swOld = Stopwatch.StartNew();
for (var i = 0; i < iterations; i++) { /* old per-item loop */ }
swOld.Stop();

var swNew = Stopwatch.StartNew();
for (var i = 0; i < iterations; i++) { /* new batched call */ }
swNew.Stop();

return Results.Ok(new { oldAvgMs = swOld.Elapsed.TotalMilliseconds / iterations, newAvgMs = swNew.Elapsed.TotalMilliseconds / iterations });
```

This is how the `CardApiController.MapToApiModels` N+1 fix was validated: HTTP load
testing swung ±25% between identical-code runs (useless signal), but the in-process A/B
comparison consistently showed the batched version 1.6-2.2x faster across 3 repeated
calls, isolated from environment noise.
