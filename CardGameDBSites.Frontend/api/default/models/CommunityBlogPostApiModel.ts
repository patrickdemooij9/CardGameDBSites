export type CommunityBlogPostApiModel = {
    id: string;
    title: string;
    imageUrl?: string | null;
    url: string;
    publishedDate: string;
    author: string;
    tagType: string;
};
