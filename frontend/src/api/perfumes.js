import client from './client';

export const perfumesApi = {
  getAll: async ({ search, brand, gender, accord, season, country, tier, yearFrom, yearTo, sort, page = 1, limit = 20 } = {}) => {
    const res = await client.get('/perfumes', { params: { search, brand, gender, accord, season, country, tier, yearFrom, yearTo, sort, page, limit } });
    return res.data;
  },
  getMeta: async () => {
    const res = await client.get('/perfumes/meta');
    return res.data; // { topBrands, topAccords, countries, seasons }
  },
  getById: async (id) => {
    const res = await client.get(`/perfumes/${id}`);
    return res.data;
  },
};
