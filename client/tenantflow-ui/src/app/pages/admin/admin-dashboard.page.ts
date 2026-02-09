import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';

interface TenantItem {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  userCount: number;
}

@Component({
  standalone: true,
  selector: 'app-admin-dashboard-page',
  imports: [
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './admin-dashboard.page.html',
  styleUrl: './admin-dashboard.page.scss'
})
export class AdminDashboardPage implements OnInit {
  private readonly apiBaseUrl = environment.apiBaseUrl;

  readonly tenants = signal<TenantItem[]>([]);
  newTenantName = '';
  newTenantSlug = '';

  constructor(private readonly http: HttpClient) {}

  ngOnInit(): void {
    void this.loadTenants();
  }

  async createTenant(): Promise<void> {
    const name = this.newTenantName.trim();
    const slug = this.newTenantSlug.trim().toLowerCase();
    if (!name || !slug) {
      return;
    }

    await firstValueFrom(this.http.post(this.apiBaseUrl + '/admin/tenants', { name, slug }));
    this.newTenantName = '';
    this.newTenantSlug = '';
    await this.loadTenants();
  }

  private async loadTenants(): Promise<void> {
    const tenants = await firstValueFrom(this.http.get<TenantItem[]>(this.apiBaseUrl + '/admin/tenants'));
    this.tenants.set(tenants ?? []);
  }
}
