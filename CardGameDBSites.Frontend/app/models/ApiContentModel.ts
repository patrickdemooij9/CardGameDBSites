import type { IApiContentModelBase } from "~/api/umbraco";
import type { PageSeoMetaField } from "./PageSeoModel";

export type ApiContentModel = IApiContentModelBase & {
  seoToolkit: PageSeoMetaField;
};