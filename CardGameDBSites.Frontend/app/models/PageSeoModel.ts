export interface PageSeoModel {
    scripts: string[];
    metaFields: PageSeoMetaField;
}

interface PageSeoMetaField {
    canonicalUrl: string | null;
    facebookId: string | null;
    metaDescription: string | null;
    openGraphDescription: string | null;
    openGraphImage: string | null;
    openGraphTitle: string | null;
    openGraphUrl: string | null;
    robots: string[] | null;
    schema: string | null;
    scripts: string[];
    title: string | null;
    twitterCardType: string | null;
    twitterCreator: string | null;
    twitterSite: string | null;
}