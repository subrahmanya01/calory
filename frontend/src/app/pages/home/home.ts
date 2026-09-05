import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';
import { Spinner } from '../../components/spinner/spinner';
import { UserApi } from '../../services/user-api';
import { User } from '../../interfaces/user';

@Component({
  selector: 'app-home',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Header, Footer, Spinner],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly formBuilder = inject(FormBuilder);
  readonly user = signal<User | null>(null);
  readonly loading = signal(true);
  readonly editing = signal(false);
  readonly saving = signal(false);
  readonly message = signal('');

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required], lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]], password: [''],
  });

  constructor(private readonly api: UserApi, private readonly router: Router) {
    if (!localStorage.getItem('calory_token')) { this.loading.set(false); return; }
    this.api.currentUser().subscribe({
      next: (user) => { 
        this.user.set(user);
        this.fillForm(user); 
        this.loading.set(false); 
      },
      error: () => { 
        localStorage.removeItem('calory_token'); 
        this.router.navigateByUrl('/login'); 
      },
    });
  }

  private fillForm(user: User): void { this.form.patchValue({ firstName: user.firstName, lastName: user.lastName, email: user.email, password: '' }); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.message.set('');
    const value = this.form.getRawValue();
    this.api.update({ ...value, password: value.password || undefined }).subscribe({
      next: (user) => { 
        this.user.set(user); this.fillForm(user); 
        this.editing.set(false); 
        this.saving.set(false); 
        this.message.set('Your profile is up to date.'); 
      },
      error: () => { 
        this.saving.set(false); 
        this.message.set('We could not save those changes.'); 
      },
    });
  }

  removeAccount(): void {
    if (!window.confirm('Deactivate your Calory account?')) return;
    this.api.delete().subscribe({ 
      next: () => { 
        localStorage.removeItem('calory_token'); 
        this.router.navigateByUrl('/login'); 
      }, 
        error: () => this.message.set('We could not deactivate your account.') 
      });
  }

}
