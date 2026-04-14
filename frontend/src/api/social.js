import client from './client';

export const socialApi = {
  toggleLike: async (perfumeId) => {
    const res = await client.post(`/social/like/${perfumeId}`);
    return res.data; // { liked, count }
  },
  getLikedIds: async (perfumeIds) => {
    const res = await client.get('/social/likes', { params: { perfumeIds: perfumeIds.join(',') } });
    return res.data; // number[]
  },
  toggleFollow: async (userId) => {
    const res = await client.post(`/social/follow/${userId}`);
    return res.data; // { following }
  },
  getUser: async (username) => {
    const res = await client.get(`/social/users/${username}`);
    return res.data;
  },
  getUserCollection: async (username, status) => {
    const res = await client.get(`/social/users/${username}/collection`, { params: status ? { status } : {} });
    return res.data;
  },
  getFeed: async () => {
    const res = await client.get('/social/feed');
    return res.data;
  },
};
