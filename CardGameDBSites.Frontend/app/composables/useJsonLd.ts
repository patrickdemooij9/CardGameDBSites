import { serializeJsonLd, type JsonLdNode } from "~/helpers/StructuredDataHelper";

export function useJsonLd(nodes: MaybeRefOrGetter<JsonLdNode[]>) {
  useHead(() => ({
    script: [{ type: "application/ld+json", innerHTML: serializeJsonLd(toValue(nodes)) }],
  }));
}
