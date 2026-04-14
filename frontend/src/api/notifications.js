import client from './client';

export const notificationsApi = {
  getAll: async ({ page = 1, limit = 20 } = {}) => {
    const res = await client.get('/notifications', { params: { page, limit } });
    return res.data; // { notifications: [], total, unread }
  },
  getUnreadCount: async () => {
    const res = await client.get('/notifications/unread-count');
    return res.data.count;
  },
  markRead: async (id) => {
    const res = await client.patch(`/notifications/${id}/read`);
    return res.data;
  },
  markAllRead: async () => {
    const res = await client.patch('/notifications/read-all');
    return res.data;
  },
};
