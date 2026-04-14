import client from './client';

export const communitiesApi = {
  getAll: async () => {
    const res = await client.get('/communities');
    return res.data;
  },
  getOne: async (id) => {
    const res = await client.get(`/communities/${id}`);
    return res.data;
  },
  create: async ({ name, description, imageUrl }) => {
    const res = await client.post('/communities', { name, description, imageUrl });
    return res.data;
  },
  toggleJoin: async (id) => {
    const res = await client.post(`/communities/${id}/join`);
    return res.data; // { joined }
  },
  getPosts: async (id, page = 1) => {
    const res = await client.get(`/communities/${id}/posts`, { params: { page } });
    return res.data; // { posts, total }
  },
  createPost: async (id, body, perfumeId = null) => {
    const res = await client.post(`/communities/${id}/posts`, { body, perfumeId });
    return res.data;
  },
  deletePost: async (communityId, postId) => {
    await client.delete(`/communities/${communityId}/posts/${postId}`);
  },
};
