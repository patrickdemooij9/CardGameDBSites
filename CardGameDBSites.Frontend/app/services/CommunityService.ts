import type { PagedCommunityBlogPostsApiModel } from '~/api/default';
import { DoFetch } from '~/helpers/RequestsHelper';

export default class CommunityService {
    async getPosts(page: number = 1, pageSize: number = 30): Promise<PagedCommunityBlogPostsApiModel> {
        return DoFetch<PagedCommunityBlogPostsApiModel>(`/api/community/posts?page=${page}&pageSize=${pageSize}`);
    }
}
