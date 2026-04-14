import axios from 'axios';

const client = axios.create({
  baseURL: 'http://localhost:3000/api',
});

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

client.interceptors.response.use(
  (res) => res,
  (err) => {
    const message = err.response?.data?.error || 'Bir hata oluştu';
    return Promise.reject(new Error(message));
  }
);

export default client;
