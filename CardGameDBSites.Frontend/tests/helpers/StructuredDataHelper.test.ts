import { describe, expect, it } from "vitest";
import {
  buildBreadcrumbSchema,
  buildWebPageSchema,
  buildWebSiteSchema,
  serializeJsonLd,
  siteNameFromTitle,
} from "~/helpers/StructuredDataHelper";

const origin = "https://sw-unlimited-db.com";

describe("buildBreadcrumbSchema", () => {
  it("links home, the section and the current page", () => {
    const schema = buildBreadcrumbSchema(origin, "/cards/HMW/darth-vader", "Darth Vader");

    expect(schema.itemListElement).toEqual([
      { "@type": "ListItem", position: 1, name: "Home", item: `${origin}/` },
      { "@type": "ListItem", position: 2, name: "Cards", item: `${origin}/cards/` },
      { "@type": "ListItem", position: 3, name: "Darth Vader", item: `${origin}/cards/HMW/darth-vader` },
    ]);
  });

  it("title-cases hyphenated sections", () => {
    const schema = buildBreadcrumbSchema(origin, "/twin-suns-decks/123", "My deck");
    const items = schema.itemListElement as { name: string }[];

    expect(items[1]!.name).toBe("Twin Suns Decks");
  });

  it("shares its id with the web page breadcrumb reference", () => {
    const pageUrl = `${origin}/decks/1`;
    const breadcrumb = buildBreadcrumbSchema(origin, "/decks/1", "Deck");
    const page = buildWebPageSchema(pageUrl, "Deck");

    expect(page.breadcrumb).toEqual({ "@id": breadcrumb["@id"] });
  });
});

describe("serializeJsonLd", () => {
  it("escapes angle brackets so user text cannot close the script tag", () => {
    const json = serializeJsonLd([{ name: "</script><script>alert(1)</script>" }]);

    expect(json).not.toContain("</script>");
    expect(JSON.parse(json)["@graph"][0].name).toBe("</script><script>alert(1)</script>");
  });

  it("wraps nodes in a schema.org graph", () => {
    const parsed = JSON.parse(serializeJsonLd([buildWebSiteSchema(origin, "SW-Unlimited-DB")]));

    expect(parsed["@context"]).toBe("https://schema.org");
    expect(parsed["@graph"][0]).toMatchObject({
      "@type": "WebSite",
      name: "SW-Unlimited-DB",
      alternateName: "sw-unlimited-db.com",
    });
  });
});

describe("siteNameFromTitle", () => {
  it("takes the part before the first pipe", () => {
    expect(siteNameFromTitle("SW-Unlimited-DB | Star Wars Unlimited Database", "x")).toBe("SW-Unlimited-DB");
  });

  it("falls back when the title is empty", () => {
    expect(siteNameFromTitle("", "sw-unlimited-db.com")).toBe("sw-unlimited-db.com");
    expect(siteNameFromTitle(undefined, "sw-unlimited-db.com")).toBe("sw-unlimited-db.com");
  });
});
