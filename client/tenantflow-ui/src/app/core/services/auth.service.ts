import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthResponse, DemoLoginRequest, LoginRequest } from '../models/auth.models';
import { environment } from '../../../environments/environment';

const AUTH_KEY = 'tenantflow_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiBaseUrl = environment.apiBaseUrl;
  private readonly authState = signal<AuthResponse | null>(null);

  readonly token = computed(() => this.authState()?.token ?? null);
  readonly user = computed(() => this.authState());
  readonly isAuthenticated = computed(() => this.authState() !== null);
  readonly roles = computed(() => this.authState()?.roles ?? []);
  readonly isPlatformAdmin = computed(() => this.hasAnyRole(['PlatformAdmin']));

  constructor(private readonly http: HttpClient, private readonly router: Router) {
    this.restore();
  }

  async login(request: LoginRequest): Promise<void> {
    const response = await firstValueFrom(this.http.post<AuthResponse>(this.apiBaseUrl + '/auth/login', request));
    this.setAuth(response);
  }

  async demoLogin(persona: DemoLoginRequest['persona']): Promise<void> {
    const response = await firstValueFrom(this.http.post<AuthResponse>(this.apiBaseUrl + '/auth/demo', { persona }));
    this.setAuth(response);
  }

  logout(): void {
    void firstValueFrom(this.http.post<void>(this.apiBaseUrl + '/auth/logout', {})).catch(() => undefined);
    this.authState.set(null);
    sessionStorage.removeItem(AUTH_KEY);
    void this.router.navigateByUrl('/login');
  }

  hasAnyRole(roles: string[]): boolean {
    const currentRoles = this.roles().map((role) => role.toLowerCase());
    return roles.some((role) => currentRoles.includes(role.toLowerCase()));
  }

  private setAuth(response: AuthResponse): void {
    const safeAuth: AuthResponse = {
      ...response,
      token: ''
    };

    this.authState.set(safeAuth);
    sessionStorage.setItem(AUTH_KEY, JSON.stringify(safeAuth));
  }

  private restore(): void {
    const rawAuth = sessionStorage.getItem(AUTH_KEY);
    if (!rawAuth) {
      return;
    }

    try {
      const parsed = JSON.parse(rawAuth) as AuthResponse;
      this.authState.set(parsed);
    } catch {
      sessionStorage.removeItem(AUTH_KEY);
    }
  }
}
