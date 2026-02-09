import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

interface TenantUser {
  userId: string;
  email: string;
  displayName: string;
  role: string;
  isActive: boolean;
}

interface FeatureFlag {
  id: string;
  key: string;
  isEnabled: boolean;
  rolloutPercent: number;
}

interface Quote {
  id: string;
  quoteNumber: string;
  customerName: string;
  premium: number;
}

@Component({
  standalone: true,
  selector: 'app-tenant-dashboard-page',
  imports: [
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './tenant-dashboard.page.html',
  styleUrl: './tenant-dashboard.page.scss'
})
export class TenantDashboardPage implements OnInit {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  readonly users = signal<TenantUser[]>([]);
  readonly flags = signal<FeatureFlag[]>([]);
  readonly quotes = signal<Quote[]>([]);
  readonly canManageUsers = computed(() => this.auth.hasAnyRole(['TenantAdmin', 'PlatformAdmin']));

  email = '';
  displayName = '';
  role = 'TenantUser';

  customerName = '';
  premium = 1000;

  constructor(private readonly http: HttpClient, private readonly auth: AuthService) {}

  ngOnInit(): void {
    void this.refreshAll();
  }

  async createUser(): Promise<void> {
    if (!this.canManageUsers()) {
      return;
    }

    await firstValueFrom(this.http.post(this.apiBaseUrl + '/tenant/users', {
      email: this.email,
      displayName: this.displayName,
      role: this.role
    }));

    this.email = '';
    this.displayName = '';
    this.role = 'TenantUser';
    await this.loadUsers();
  }

  async createQuote(): Promise<void> {
    await firstValueFrom(this.http.post(this.apiBaseUrl + '/quotes', {
      customerName: this.customerName,
      premium: Number(this.premium)
    }));

    this.customerName = '';
    this.premium = 1000;
    await this.loadQuotes();
  }

  private async refreshAll(): Promise<void> {
    await Promise.all([this.loadUsers(), this.loadFlags(), this.loadQuotes()]);
  }

  private async loadUsers(): Promise<void> {
    const users = await firstValueFrom(this.http.get<TenantUser[]>(this.apiBaseUrl + '/tenant/users'));
    this.users.set(users ?? []);
  }

  private async loadFlags(): Promise<void> {
    const flags = await firstValueFrom(this.http.get<FeatureFlag[]>(this.apiBaseUrl + '/tenant/feature-flags'));
    this.flags.set(flags ?? []);
  }

  private async loadQuotes(): Promise<void> {
    const quotes = await firstValueFrom(this.http.get<Quote[]>(this.apiBaseUrl + '/quotes'));
    this.quotes.set(quotes ?? []);
  }
}
