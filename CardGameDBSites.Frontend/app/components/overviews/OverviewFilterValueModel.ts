import type { CardSearchFilterMode } from "~/api/default";

export default interface OverviewFilterValueModel {
    Alias: string;
    Values: string[];
    Mode: CardSearchFilterMode;
}