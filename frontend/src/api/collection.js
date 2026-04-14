import client from './client';

export const collectionApi = {
  getAll: async (status) => {
    const res = await client.get('/collection', { params: status ? { status } : {} });
    return res.data;
  },
  add: async (perfumeId, status, bottleLevel) => {
    const res = await client.post('/collection', { perfumeId, status, bottleLevel });
    return res.data;
  },
  update: async (perfumeId, data) => {
    const res = await client.patch(`/collection/${perfumeId}`, data);
    return res.data;
  },
  remove: async (perfumeId) => {
    await client.delete(`/collection/${perfumeId}`);
  },
};
