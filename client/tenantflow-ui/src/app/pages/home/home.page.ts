import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  selector: 'app-home-page',
  imports: [RouterLink, MatCardModule, MatButtonModule],
  templateUrl: './home.page.html',
  styleUrl: './home.page.scss'
})
export class HomePage {}
