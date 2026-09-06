import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { User } from '../../interfaces/user';
import { UserApi } from '../../services/user-api';
import { Spinner } from '../../components/spinner/spinner';

@Component({
  selector: 'app-user-profile',
  imports: [ReactiveFormsModule, Spinner],
  templateUrl: './user-profile.html',
  styleUrl: './user-profile.css',
})
export class UserProfile {
  @Input() user: User | null = null;
  @Output() saved = new EventEmitter<User>();
  @Output() closed = new EventEmitter<void>();
  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(UserApi);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
  });

  ngOnChanges(): void {
    if (this.user)
      this.form.patchValue({
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        password: '',
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.error.set('');
    const value = this.form.getRawValue();
    this.api.update({ ...value, password: value.password || undefined }).subscribe({
      next: (user) => {
        this.saving.set(false);
        this.saved.emit(user);
      },
      error: () => {
        this.saving.set(false);
        this.error.set('We could not save your profile.');
      },
    });
  }
}
