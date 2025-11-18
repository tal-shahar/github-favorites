export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
  username?: string;
  avatarUrl?: string;
}

export interface AuthState {
  email: string;
  token: string;
  expiresAtUtc: string;
  username?: string;
  avatarUrl?: string;
}

