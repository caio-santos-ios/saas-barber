import axios from 'axios';
import { environment } from '../../environments/environment';

export const api = axios.create({
  baseURL: environment.apiUrl || 'http://localhost:5056',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});
