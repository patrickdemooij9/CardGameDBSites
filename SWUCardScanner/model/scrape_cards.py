"""Download every Star Wars: Unlimited card image from api.sw-unlimited-db.com.

The API returns one row per card *variant* (~8.6k rows), but many variants share the
same artwork, so images are de-duplicated by media URL. Leaders and bases are
landscape; units, events and upgrades are portrait. Double-sided cards (leaders)
expose a second image via `backImageUrl`, which is downloaded as its own entry.

Images come back as RGBA PNGs whose alpha already carries the rounded card corners,
and the card rectangle spans the full image - so the four corners of the image *are*
the four corners of the card. build_dataset.py relies on that.

Usage:
    python scrape_cards.py                    # -> cards/ + cards.json
    python scrape_cards.py --width 512        # larger art
    python scrape_cards.py --limit 200        # quick smoke test
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

import requests
from tqdm import tqdm

API_BASE = "https://api.sw-unlimited-db.com"
QUERY_URL = f"{API_BASE}/api/cards/query"
PAGE_SIZE = 500

# Card types that are printed landscape. Everything else is portrait.
LANDSCAPE_TYPES = {"leader", "base"}

HERE = os.path.dirname(os.path.abspath(__file__))


def _session() -> requests.Session:
    s = requests.Session()
    s.headers.update({"User-Agent": "SWUCardScanner/1.0 (dataset builder)"})
    return s


def fetch_all_variants(session: requests.Session, limit: int | None = None) -> list[dict]:
    """Page through /api/cards/query and return every card variant row."""
    items: list[dict] = []
    page = 1
    total_pages = 1

    with tqdm(desc="Fetching card list", unit="page") as bar:
        while page <= total_pages:
            resp = session.post(
                QUERY_URL,
                json={"pageNumber": page, "pageSize": PAGE_SIZE, "filterClauses": []},
                timeout=60,
            )
            resp.raise_for_status()
            data = resp.json()

            total_pages = data.get("totalPages", 1)
            bar.total = total_pages
            items.extend(data.get("items", []))
            bar.update(1)

            if limit is not None and len(items) >= limit:
                return items[:limit]
            page += 1

    return items


def _safe_name(media_url: str) -> str:
    """Turn .../media/peobven4/some-card-unit.png into peobven4_some-card-unit.png.

    The Umbraco media key is unique per file, so this can never collide even when two
    different cards share a display name.
    """
    path = media_url.split("?", 1)[0]
    parts = [p for p in path.split("/") if p]
    tail = parts[-2:] if len(parts) >= 2 else parts[-1:]
    name = "_".join(tail)
    name = re.sub(r"[^A-Za-z0-9._-]", "-", name)
    if not name.lower().endswith(".png"):
        name += ".png"
    return name


def _first(attrs: dict, key: str, default: str = "") -> str:
    values = attrs.get(key) or []
    return values[0] if values else default


def build_manifest(variants: list[dict]) -> list[dict]:
    """Collapse variant rows into a de-duplicated list of downloadable card faces."""
    by_url: dict[str, dict] = {}

    for item in variants:
        attrs = item.get("attributes") or {}
        card_type = _first(attrs, "Card Type", "Unknown")

        faces = [("front", item.get("imageUrl")), ("back", item.get("backImageUrl"))]
        for face, image in faces:
            if not image or not image.get("url"):
                continue
            url = image["url"].split("?", 1)[0]
            if url in by_url:
                # Remember that another variant reuses this artwork.
                by_url[url]["variantIds"].append(item.get("variantId"))
                continue

            # A leader's front is its unit side (portrait); the back is the landscape
            # leader side. Bases are always landscape. Everything else is portrait.
            if card_type.lower() == "leader":
                orientation = "landscape" if face == "back" else "portrait"
            elif card_type.lower() in LANDSCAPE_TYPES:
                orientation = "landscape"
            else:
                orientation = "portrait"

            by_url[url] = {
                "file": _safe_name(url),
                "url": url,
                "face": face,
                "baseId": item.get("baseId"),
                "variantIds": [item.get("variantId")],
                "name": item.get("displayName"),
                "setName": item.get("setName"),
                "urlSegment": item.get("urlSegment"),
                "cardType": card_type,
                "rarity": _first(attrs, "Rarity"),
                "swuId": _first(attrs, "SWU Id"),
                "orientation": orientation,
            }

    return list(by_url.values())


def download_one(session: requests.Session, entry: dict, out_dir: str, width: int,
                 overwrite: bool) -> tuple[str, bool, str | None]:
    dest = os.path.join(out_dir, entry["file"])
    if os.path.exists(dest) and not overwrite and os.path.getsize(dest) > 0:
        return entry["file"], True, None

    url = f"{entry['url']}?width={width}"
    for attempt in range(3):
        try:
            resp = session.get(url, timeout=90)
            resp.raise_for_status()
            tmp = dest + ".part"
            with open(tmp, "wb") as fh:
                fh.write(resp.content)
            os.replace(tmp, dest)
            return entry["file"], True, None
        except Exception as exc:  # noqa: BLE001 - report and continue
            if attempt == 2:
                return entry["file"], False, str(exc)
            time.sleep(1.5 * (attempt + 1))
    return entry["file"], False, "unreachable"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--out", default=os.path.join(HERE, "cards"),
                        help="directory to write card PNGs into (default: model/cards)")
    parser.add_argument("--manifest", default=os.path.join(HERE, "cards.json"),
                        help="where to write the card metadata manifest")
    parser.add_argument("--width", type=int, default=440,
                        help="server-side resize width in px (default 440; alpha is preserved)")
    parser.add_argument("--workers", type=int, default=8, help="parallel downloads")
    parser.add_argument("--limit", type=int, default=None,
                        help="only process the first N variant rows (smoke test)")
    parser.add_argument("--overwrite", action="store_true", help="re-download existing files")
    args = parser.parse_args()

    os.makedirs(args.out, exist_ok=True)
    session = _session()

    variants = fetch_all_variants(session, args.limit)
    manifest = build_manifest(variants)
    print(f"{len(variants)} variants -> {len(manifest)} unique card images")

    failures: list[tuple[str, str]] = []
    with ThreadPoolExecutor(max_workers=args.workers) as pool:
        futures = [
            pool.submit(download_one, session, entry, args.out, args.width, args.overwrite)
            for entry in manifest
        ]
        for fut in tqdm(as_completed(futures), total=len(futures), desc="Downloading", unit="img"):
            name, ok, err = fut.result()
            if not ok:
                failures.append((name, err or "unknown"))

    ok_files = {e["file"] for e in manifest} - {f for f, _ in failures}
    manifest = [e for e in manifest if e["file"] in ok_files]

    with open(args.manifest, "w", encoding="utf-8") as fh:
        json.dump(manifest, fh, indent=1, ensure_ascii=False)

    portrait = sum(1 for e in manifest if e["orientation"] == "portrait")
    print(f"\nSaved {len(manifest)} images to {args.out}")
    print(f"  portrait: {portrait}   landscape: {len(manifest) - portrait}")
    print(f"  manifest: {args.manifest}")
    if failures:
        print(f"  {len(failures)} downloads failed, e.g. {failures[:3]}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
