export default interface MemberModel {
    id?: number;
    name?: string;
    likedDecks?: number[];
    isAdmin?: boolean;
    impersonatedBy?: number | null;
}