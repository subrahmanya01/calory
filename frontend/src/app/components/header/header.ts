import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  constructor(private readonly router: Router) {}

  get signedIn(): boolean { return !!localStorage.getItem('calory_token'); }

  logout(): void {
    localStorage.removeItem('calory_token');
    this.router.navigateByUrl('/login');
  }

}
