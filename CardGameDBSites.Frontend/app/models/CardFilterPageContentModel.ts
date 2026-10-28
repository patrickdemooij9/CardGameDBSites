import type { IApiContentModelBase, RichTextModel } from "~/api/umbraco";

// Replace with the generated CardFilterCollection/LandingContentModel after running generateUmbracoApi.
export type CardFilterPageContentModel = IApiContentModelBase & {
  properties?: {
    title?: string | null;
    description?: RichTextModel | null;
    content?: RichTextModel | null;
    filterValue?: string | null;
  };
};
