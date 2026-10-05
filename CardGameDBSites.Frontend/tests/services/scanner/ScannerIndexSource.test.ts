import { beforeEach, describe, expect, it } from "vitest";
import { resolveScannerIndex, type FetchFile } from "~/services/scanner/ScannerIndexSource";

const REMOTE = "https://scanner.test";
const BUNDLED = "/scanner/index";

class MemoryCache {
  entries = new Map<string, Response>();

  async match(url: string) {
    return this.entries.get(url)?.clone();
  }

  async put(url: string, response: Response) {
    this.entries.set(url, new Response(await response.arrayBuffer(), { status: response.status }));
  }

  async keys() {
    return [...this.entries.keys()].map((url) => ({ url }));
  }

  async delete(request: { url: string }) {
    return this.entries.delete(request.url);
  }
}

function storageWith(cache: MemoryCache) {
  return { open: async () => cache } as unknown as CacheStorage;
}

function publishedIndex(version: string) {
  const base = `${REMOTE}/index/${version}`;
  return {
    [`${base}/card-index.json`]: JSON.stringify({ config: { embedder: "embedder.onnx" }, cards: [] }),
    [`${base}/card-index.bin`]: "index",
    [`${base}/card-pca.bin`]: "pca",
    [`${base}/embedder.onnx`]: "embedder",
  };
}

function server(files: Record<string, string>, latestVersion?: string) {
  const requested: string[] = [];
  const fetchFn: FetchFile = async (url) => {
    requested.push(url);
    if (url.startsWith(`${REMOTE}/index/latest.json`)) {
      return latestVersion
        ? new Response(JSON.stringify({ version: latestVersion }))
        : new Response(null, { status: 404 });
    }
    const body = files[url];
    return body === undefined ? new Response(null, { status: 404 }) : new Response(body);
  };
  return { fetchFn, requested };
}

const offline: FetchFile = async () => {
  throw new Error("offline");
};

describe("resolveScannerIndex", () => {
  let cache: MemoryCache;

  beforeEach(() => {
    cache = new MemoryCache();
  });

  it("uses the bundled index when no remote is configured", async () => {
    const source = await resolveScannerIndex(undefined, BUNDLED, storageWith(cache), offline);
    expect(source.base).toBe(BUNDLED);
  });

  it("downloads and serves the latest published version from the cache", async () => {
    const { fetchFn } = server(publishedIndex("v2"), "v2");

    const source = await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), fetchFn);

    expect(source.base).toBe(`${REMOTE}/index/v2`);
    expect(cache.entries.size).toBe(4);
    const embedder = await source.fetchFile(`${REMOTE}/index/v2/embedder.onnx`);
    expect(await embedder.text()).toBe("embedder");
  });

  it("does not download again when the latest version is already cached", async () => {
    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(publishedIndex("v2"), "v2").fetchFn);
    const { fetchFn, requested } = server(publishedIndex("v2"), "v2");

    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), fetchFn);

    expect(requested).toHaveLength(1);
  });

  it("replaces an older cached version", async () => {
    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(publishedIndex("v1"), "v1").fetchFn);

    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(publishedIndex("v2"), "v2").fetchFn);

    expect([...cache.entries.keys()].every((url) => url.startsWith(`${REMOTE}/index/v2/`))).toBe(true);
  });

  it("falls back to the cached version when offline", async () => {
    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(publishedIndex("v1"), "v1").fetchFn);

    const source = await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), offline);

    expect(source.base).toBe(`${REMOTE}/index/v1`);
  });

  it("keeps the cached version when a new download is incomplete", async () => {
    await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(publishedIndex("v1"), "v1").fetchFn);
    const broken = publishedIndex("v2");
    delete broken[`${REMOTE}/index/v2/card-pca.bin`];

    const source = await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), server(broken, "v2").fetchFn);

    expect(source.base).toBe(`${REMOTE}/index/v1`);
    expect(await cache.match(`${REMOTE}/index/v2/card-index.json`)).toBeUndefined();
  });

  it("falls back to the bundled index when offline with nothing cached", async () => {
    const source = await resolveScannerIndex(REMOTE, BUNDLED, storageWith(cache), offline);
    expect(source.base).toBe(BUNDLED);
  });
});
