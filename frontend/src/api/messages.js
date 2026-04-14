import client from './client';

export const messagesApi = {
  getConversations: async () => {
    const res = await client.get('/messages/conversations');
    return res.data;
  },
  getThread: async (username) => {
    const res = await client.get(`/messages/${username}`);
    return res.data; // { partner, messages }
  },
  send: async (username, body) => {
    const res = await client.post(`/messages/${username}`, { body });
    return res.data;
  },
};
