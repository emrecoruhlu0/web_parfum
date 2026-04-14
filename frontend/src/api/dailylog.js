import client from './client';

export const dailylogApi = {
  getByMonth: async (month) => {
    const res = await client.get('/dailylog', { params: { month } });
    return res.data;
  },
  add: async (perfumeId, date, note, sprays) => {
    const res = await client.post('/dailylog', { perfumeId, date, note, sprays });
    return res.data;
  },
  remove: async (id) => {
    await client.delete(`/dailylog/${id}`);
  },
  getProfile: async () => {
    const res = await client.get('/dailylog/profile');
    return res.data;
  },
};
