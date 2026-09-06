import { Component, ElementRef, HostListener, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { User } from '../../interfaces/user';
import { UserApi } from '../../services/user-api';
import { UserProfile } from '../../pages/user-profile/user-profile';

@Component({
  selector: 'app-header',
  imports: [RouterLink, UserProfile],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  readonly menuOpen = signal(false);
  readonly profileOpen = signal(false);
  readonly user = signal<User | null>(null);

  constructor(
    private readonly router: Router,
    private readonly userApi: UserApi,
    private readonly elementRef: ElementRef<HTMLElement>,
  ) {
    if (this.signedIn)
      this.userApi.currentUser().subscribe({ next: (user) => this.user.set(user) });
  }

  get signedIn(): boolean {
    return !!localStorage.getItem('calory_token');
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  @HostListener('document:click', ['$event'])
  closeMenuWhenClickingOutside(event: MouseEvent): void {
    const target = event.target as Node | null;
    if (target && !this.elementRef.nativeElement.contains(target)) this.menuOpen.set(false);
  }

  openProfile(): void {
    this.menuOpen.set(false);
    this.profileOpen.set(true);
  }
  profileSaved(user: User): void {
    this.user.set(user);
    this.profileOpen.set(false);
  }

  logout(): void {
    this.menuOpen.set(false);
    localStorage.removeItem('calory_token');
    this.router.navigateByUrl('/login');
  }
}
