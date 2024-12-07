import axios, { AxiosError, AxiosInstance, AxiosRequestConfig } from 'axios';
import { getAuthToken, refreshToken } from '@/lib/auth/token';

export class ApiError extends Error {
  constructor(
    public status: number,
    public message: string,
    public errors?: Record<string, string[]>
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export const createApiClient = (): AxiosInstance => {
  const client = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_URL,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  client.interceptors.request.use(async (config) => {
    const token = await getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
      
      if (error.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;
        try {
          await refreshToken();
          const token = await getAuthToken();
          if (token && originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${token}`;
          }
          return client(originalRequest);
        } catch (refreshError) {
          return Promise.reject(refreshError);
        }
      }

      if (error.response?.data) {
        throw new ApiError(
          error.response.status,
          error.response.data.message || 'An error occurred',
          error.response.data.errors
        );
      }

      throw new ApiError(
        error.response?.status || 500,
        error.message || 'Network error occurred'
      );
    }
  );

  return client;
};

export const apiClient = createApiClient();
