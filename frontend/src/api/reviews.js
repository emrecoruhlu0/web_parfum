import client from './client';

export const reviewsApi = {
  add: async (perfumeId, rating, body) => {
    const res = await client.post('/reviews', { perfumeId, rating, body });
    return res.data;
  },
  remove: async (id) => {
    await client.delete(`/reviews/${id}`);
  },
};
