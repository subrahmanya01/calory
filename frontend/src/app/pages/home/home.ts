import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';
import { Spinner } from '../../components/spinner/spinner';
import { UserApi } from '../../services/user-api';
import { User } from '../../interfaces/user';
import { GoalEditor } from '../../components/goal-editor/goal-editor';
import { FoodEntryEditor } from '../../components/food-entry-editor/food-entry-editor';
import { ImageFoodReview } from '../../components/image-food-review/image-food-review';
import { HealthGoal } from '../../interfaces/health-goal';
import { FoodEntry } from '../../interfaces/food-entry';
import { DailyNutrition, TrendPoint } from '../../interfaces/report';
import { HealthGoalApi } from '../../services/health-goal-api';
import { FoodEntryApi } from '../../services/food-entry-api';
import { ReportApi } from '../../services/report-api';
import { Chat } from '../../components/chat/chat';

@Component({
  selector: 'app-home',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Header, Footer, Spinner, GoalEditor, FoodEntryEditor, ImageFoodReview, Chat],
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
  readonly goals = signal<HealthGoal[]>([]);
  readonly entries = signal<FoodEntry[]>([]);
  readonly daily = signal<DailyNutrition[]>([]);
  readonly trends = signal<TrendPoint[]>([]);
  readonly showGoalEditor = signal(false);
  readonly showFoodEditor = signal(false);
  readonly showImageReview = signal(false);
  readonly editingGoal = signal<HealthGoal | null>(null);
  readonly editingEntry = signal<FoodEntry | null>(null);
  readonly chatOpen = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required], lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]], password: [''],
  });

  constructor(private readonly api: UserApi, private readonly router: Router, private readonly goalApi: HealthGoalApi, private readonly foodApi: FoodEntryApi, private readonly reportApi: ReportApi) {
    if (!localStorage.getItem('calory_token')) { this.loading.set(false); return; }
    this.api.currentUser().subscribe({
      next: (user) => { 
        this.user.set(user);
        this.fillForm(user); 
        this.loading.set(false);
        this.loadHealthData();
      },
      error: () => { 
        localStorage.removeItem('calory_token'); 
        this.router.navigateByUrl('/login'); 
      },
    });
  }

  get activeGoal(): HealthGoal | null { return this.goals().find((goal) => goal.isActive) ?? this.goals()[0] ?? null; }
  get todayCalories(): number { return this.daily().find((day) => day.date === this.today())?.totals.calories ?? 0; }
  get calorieProgress(): number { const target = this.activeGoal?.dailyCalorieTarget ?? 2000; return Math.min(100, Math.round((this.todayCalories / target) * 100)); }
  get trendPeak(): number { return Math.max(...this.trends().map((point) => point.calories), 1); }

  openNewGoal(): void { this.editingGoal.set(null); this.showGoalEditor.set(true); }
  editGoal(): void { this.editingGoal.set(this.activeGoal); this.showGoalEditor.set(true); }
  openNewFood(): void { this.editingEntry.set(null); this.showFoodEditor.set(true); }
  editEntry(entry: FoodEntry): void { this.editingEntry.set(entry); this.showFoodEditor.set(true); }
  deleteEntry(entry: FoodEntry): void { if (!window.confirm(`Delete ${entry.foodName}?`)) return; this.foodApi.remove(entry.id).subscribe({ next: () => { this.message.set('Entry removed.'); this.loadHealthData(); }, error: () => this.message.set('We could not remove that entry.') }); }
  onGoalSaved(): void { this.showGoalEditor.set(false); this.message.set('Your goal is saved.'); this.loadHealthData(); }
  onFoodSaved(): void { this.showFoodEditor.set(false); this.showImageReview.set(false); this.message.set('Your food record is saved.'); this.loadHealthData(); }

  private loadHealthData(): void {
    const from = new Date(Date.now() - 6 * 86400000).toISOString().slice(0, 10);
    const to = this.today();
    this.goalApi.getAll().subscribe({ next: (goals) => this.goals.set(goals) });
    this.foodApi.list(from, to).subscribe({ next: (entries) => this.entries.set(entries) });
    this.reportApi.daily(from, to).subscribe({ next: (daily) => this.daily.set(daily) });
    this.reportApi.trends(from, to).subscribe({ next: (trends) => this.trends.set(trends) });
  }

  private today(): string { return new Date().toISOString().slice(0, 10); }

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
