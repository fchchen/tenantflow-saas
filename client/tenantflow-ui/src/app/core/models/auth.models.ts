export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  tenantId: string;
  userId: string;
  email: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
  tenantSlug?: string;
}

export interface DemoLoginRequest {
  persona: 'platform-admin' | 'tenant-admin' | 'tenant-user' | 'globex-admin';
}
