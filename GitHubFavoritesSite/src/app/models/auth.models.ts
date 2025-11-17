export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
}

export interface AuthState {
  email: string;
  token: string;
  expiresAtUtc: string;
}

