export type JsonLdNode = Record<string, unknown>;

// Escaping "<" keeps user-written text (deck names) from closing the <script> tag.
export function serializeJsonLd(nodes: JsonLdNode[]): string {
  return JSON.stringify({ "@context": "https://schema.org", "@graph": nodes }).replace(/</g, "\\u003c");
}

export function toAbsoluteUrl(url: string | null | undefined, origin: string): string | undefined {
  return url ? new URL(url, origin).toString() : undefined;
}

export function siteNameFromTitle(title: string | null | undefined, fallback: string): string {
  const name = title?.split("|")[0]?.trim();
  return name ? name : fallback;
}

function toTitleCase(segment: string): string {
  return segment
    .split("-")
    .filter(Boolean)
    .map((word) => word[0]!.toUpperCase() + word.slice(1))
    .join(" ");
}

export function buildBreadcrumbSchema(origin: string, pathname: string, currentName: string): JsonLdNode {
  const section = pathname.split("/").filter(Boolean)[0];
  const items = [{ name: "Home", url: `${origin}/` }];
  if (section) items.push({ name: toTitleCase(section), url: `${origin}/${section}/` });
  items.push({ name: currentName, url: `${origin}${pathname}` });

  return {
    "@type": "BreadcrumbList",
    "@id": `${origin}${pathname}#breadcrumb`,
    itemListElement: items.map((item, index) => ({
      "@type": "ListItem",
      position: index + 1,
      name: item.name,
      item: item.url,
    })),
  };
}

export function buildWebPageSchema(pageUrl: string, name: string, extra: JsonLdNode = {}): JsonLdNode {
  return {
    "@type": "WebPage",
    "@id": `${pageUrl}#webpage`,
    url: pageUrl,
    name,
    breadcrumb: { "@id": `${pageUrl}#breadcrumb` },
    ...extra,
  };
}

export function buildWebSiteSchema(origin: string, name: string): JsonLdNode {
  return {
    "@type": "WebSite",
    "@id": `${origin}/#website`,
    url: `${origin}/`,
    name,
    alternateName: new URL(origin).hostname,
  };
}
