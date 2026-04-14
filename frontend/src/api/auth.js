import client from './client';

export const authApi = {
  register: async (username, email, password) => {
    const res = await client.post('/auth/register', { username, email, password });
    return res.data;
  },
  login: async (email, password) => {
    const res = await client.post('/auth/login', { email, password });
    return res.data;
  },
  me: async () => {
    const res = await client.get('/auth/me');
    return res.data;
  },
};
