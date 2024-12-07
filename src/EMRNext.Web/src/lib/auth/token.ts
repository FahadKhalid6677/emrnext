import { jwtDecode } from 'jwt-decode';

const AUTH_TOKEN_KEY = 'auth_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

interface JwtPayload {
  exp: number;
  sub: string;
  roles: string[];
}

export const setAuthToken = async (token: string): Promise<void> => {
  localStorage.setItem(AUTH_TOKEN_KEY, token);
};

export const getAuthToken = async (): Promise<string | null> => {
  const token = localStorage.getItem(AUTH_TOKEN_KEY);
  if (!token) return null;

  try {
    const payload = jwtDecode<JwtPayload>(token);
    const isExpired = Date.now() >= payload.exp * 1000;
    
    if (isExpired) {
      await removeAuthToken();
      return null;
    }
    
    return token;
  } catch {
    await removeAuthToken();
    return null;
  }
};

export const removeAuthToken = async (): Promise<void> => {
  localStorage.removeItem(AUTH_TOKEN_KEY);
};

export const setRefreshToken = async (token: string): Promise<void> => {
  localStorage.setItem(REFRESH_TOKEN_KEY, token);
};

export const getRefreshToken = async (): Promise<string | null> => {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
};

export const removeRefreshToken = async (): Promise<void> => {
  localStorage.removeItem(REFRESH_TOKEN_KEY);
};

export const refreshToken = async (): Promise<void> => {
  const refreshToken = await getRefreshToken();
  if (!refreshToken) {
    throw new Error('No refresh token available');
  }

  try {
    const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/auth/refresh`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      throw new Error('Failed to refresh token');
    }

    const data = await response.json();
    await setAuthToken(data.accessToken);
    await setRefreshToken(data.refreshToken);
  } catch (error) {
    await removeAuthToken();
    await removeRefreshToken();
    throw error;
  }
};
