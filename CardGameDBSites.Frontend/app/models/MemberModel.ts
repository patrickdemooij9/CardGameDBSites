import type { MemberSettingsApiModel } from "~/api/default";

export default interface MemberModel {
    id?: number;
    name?: string;
    likedDecks?: number[];
    isAdmin?: boolean;
    impersonatedBy?: number | null;
    settings: MemberSettingsApiModel;
}