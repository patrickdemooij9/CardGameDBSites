import type { CommunityBlogPostApiModel } from './CommunityBlogPostApiModel';

export type PagedCommunityBlogPostsApiModel = {
    totalItems: number;
    page: number;
    pageSize: number;
    items: Array<CommunityBlogPostApiModel>;
};
