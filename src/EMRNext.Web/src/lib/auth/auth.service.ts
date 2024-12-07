import { apiClient } from '@/lib/api/client';
import { setAuthToken, removeAuthToken, setRefreshToken, removeRefreshToken } from './token';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserProfile;
}

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

class AuthService {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/login', credentials);
    await this.handleAuthResponse(response.data);
    return response.data;
  }

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      await this.clearAuth();
    }
  }

  async refreshAuth(refreshToken: string): Promise<RefreshTokenResponse> {
    const response = await apiClient.post<RefreshTokenResponse>('/auth/refresh', {
      refreshToken,
    });
    await setAuthToken(response.data.accessToken);
    await setRefreshToken(response.data.refreshToken);
    return response.data;
  }

  async getCurrentUser(): Promise<UserProfile> {
    const response = await apiClient.get<UserProfile>('/auth/me');
    return response.data;
  }

  private async handleAuthResponse(data: AuthResponse): Promise<void> {
    await setAuthToken(data.accessToken);
    await setRefreshToken(data.refreshToken);
  }

  private async clearAuth(): Promise<void> {
    await removeAuthToken();
    await removeRefreshToken();
  }
}

export const authService = new AuthService();
