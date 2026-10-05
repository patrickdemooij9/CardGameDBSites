const CACHE_NAME = "scanner-index";
const INDEX_JSON = "card-index.json";
const INDEX_BINARIES = ["card-index.bin", "card-pca.bin"];
const LATEST_TIMEOUT_MS = 5000;

export type FetchFile = (url: string) => Promise<Response>;

export interface ScannerIndexSource {
  base: string;
  fetchFile: FetchFile;
}

export async function resolveScannerIndex(
  remoteUrl: string | undefined,
  bundledBase: string,
  storage: CacheStorage | undefined = globalThis.caches,
  fetchFn: FetchFile = (url) => fetch(url),
): Promise<ScannerIndexSource> {
  const bundled = { base: bundledBase, fetchFile: fetchFn };
  if (!remoteUrl || !storage) {
    return bundled;
  }

  const cache = await storage.open(CACHE_NAME);
  const fromCache: FetchFile = async (url) => (await cache.match(url)) ?? fetchFn(url);

  try {
    const base = `${remoteUrl}/index/${await fetchLatestVersion(remoteUrl, fetchFn)}`;
    if (!(await isComplete(cache, base))) {
      await download(cache, base, fetchFn);
    }
    await pruneOtherVersions(cache, base);
    return { base, fetchFile: fromCache };
  } catch {
    const cached = await findCachedBase(cache);
    return cached ? { base: cached, fetchFile: fromCache } : bundled;
  }
}

async function fetchLatestVersion(remoteUrl: string, fetchFn: FetchFile): Promise<string> {
  const latest = await Promise.race([
    fetchFn(`${remoteUrl}/index/latest.json?t=${Date.now()}`),
    new Promise<never>((_, reject) => setTimeout(() => reject(new Error("Timed out")), LATEST_TIMEOUT_MS)),
  ]);
  if (!latest.ok) {
    throw new Error(`latest.json returned ${latest.status}`);
  }
  const { version } = (await latest.json()) as { version?: string };
  if (!version) {
    throw new Error("latest.json has no version");
  }
  return version;
}

async function download(cache: Cache, base: string, fetchFn: FetchFile) {
  const json = await fetchOk(fetchFn, `${base}/${INDEX_JSON}`);
  const files = [...INDEX_BINARIES, embedderOf(await json.clone().json())];
  const binaries = await Promise.all(files.map((file) => fetchOk(fetchFn, `${base}/${file}`)));

  await Promise.all(files.map((file, i) => cache.put(`${base}/${file}`, binaries[i]!)));
  // Written last, so a cached index json always means its binaries are there too.
  await cache.put(`${base}/${INDEX_JSON}`, json);
}

async function fetchOk(fetchFn: FetchFile, url: string) {
  const response = await fetchFn(url);
  if (!response.ok) {
    throw new Error(`${url} returned ${response.status}`);
  }
  return response;
}

function embedderOf(meta: { config?: { embedder?: string } }) {
  const embedder = meta.config?.embedder;
  if (!embedder) {
    throw new Error("Card index has no embedder");
  }
  return embedder;
}

async function isComplete(cache: Cache, base: string) {
  const json = await cache.match(`${base}/${INDEX_JSON}`);
  if (!json) {
    return false;
  }
  const files = [...INDEX_BINARIES, embedderOf(await json.json())];
  const hits = await Promise.all(files.map((file) => cache.match(`${base}/${file}`)));
  return hits.every(Boolean);
}

async function pruneOtherVersions(cache: Cache, base: string) {
  const stale = (await cache.keys()).filter((request) => !request.url.startsWith(`${base}/`));
  await Promise.all(stale.map((request) => cache.delete(request)));
}

async function findCachedBase(cache: Cache) {
  const suffix = `/${INDEX_JSON}`;
  for (const request of await cache.keys()) {
    if (!request.url.endsWith(suffix)) {
      continue;
    }
    const base = request.url.slice(0, -suffix.length);
    if (await isComplete(cache, base)) {
      return base;
    }
  }
  return undefined;
}
