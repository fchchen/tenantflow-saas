import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login-page',
  imports: [
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss'
})
export class LoginPage {
  email = 'admin@acme.dev';
  password = 'Pass123$';
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  async login(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.auth.login({ email: this.email, password: this.password });
      await this.routeAfterLogin();
    } catch {
      this.error.set('Login failed. Use demo credentials or one of the demo buttons.');
    } finally {
      this.loading.set(false);
    }
  }

  async demoPlatformAdmin(): Promise<void> {
    await this.demo('platform-admin');
  }

  async demoTenantAdmin(): Promise<void> {
    await this.demo('tenant-admin');
  }

  async demoTenantUser(): Promise<void> {
    await this.demo('tenant-user');
  }

  private async demo(persona: 'platform-admin' | 'tenant-admin' | 'tenant-user'): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      await this.auth.demoLogin(persona);
      await this.routeAfterLogin();
    } catch {
      this.error.set('Demo login failed.');
    } finally {
      this.loading.set(false);
    }
  }

  private async routeAfterLogin(): Promise<void> {
    if (this.auth.isPlatformAdmin()) {
      await this.router.navigateByUrl('/admin');
      return;
    }

    await this.router.navigateByUrl('/app');
  }
}
