import axios from "axios";
import { getToken, logout } from "../auth/token";

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err) => {
    const status = err.response?.status;
    if (status === 401) {
      logout("Your session expired or your account was disabled.");
      // stop here; page will redirect
    } else if (status === 403) {
      // Optional: either show a toast or also logout
      logout("Access denied.");
    }
    return Promise.reject(err);
  }
);

export default api;