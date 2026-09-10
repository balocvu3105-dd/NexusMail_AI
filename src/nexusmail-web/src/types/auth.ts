export interface LoginRequest {
  email: string;
  password: string;
  deviceName?: string | null;
  ipAddress?: string | null;
  userAgent?: string | null;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
}
