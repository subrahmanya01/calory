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
import { FoodEntry, ImportFoodEntriesResponse } from '../../interfaces/food-entry';
import { DailyNutrition, TrendPoint } from '../../interfaces/report';
import { HealthGoalApi } from '../../services/health-goal-api';
import { FoodEntryApi } from '../../services/food-entry-api';
import { ReportApi } from '../../services/report-api';
import { Chat } from '../../components/chat/chat';
import { FoodPdfImport } from '../../components/food-pdf-import/food-pdf-import';

@Component({
  selector: 'app-home',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    Header,
    Footer,
    Spinner,
    GoalEditor,
    FoodEntryEditor,
    ImageFoodReview,
    FoodPdfImport,
    Chat,
  ],
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
  readonly showPdfImport = signal(false);
  readonly editingGoal = signal<HealthGoal | null>(null);
  readonly editingEntry = signal<FoodEntry | null>(null);
  readonly chatOpen = signal(false);
  readonly chatExpanded = signal(false);
  isLoggedIn(): boolean {
    return !!this.user();
  }

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
  });

  constructor(
    private readonly api: UserApi,
    private readonly router: Router,
    private readonly goalApi: HealthGoalApi,
    private readonly foodApi: FoodEntryApi,
    private readonly reportApi: ReportApi,
  ) {
    if (!localStorage.getItem('calory_token')) {
      this.loading.set(false);
      return;
    }
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

  get activeGoal(): HealthGoal | null {
    const today = this.today();
    return (
      this.goals().find(
        (goal) => goal.startDate <= today && (!goal.endDate || goal.endDate >= today),
      ) ?? null
    );
  }
  get greeting(): string {
    const hour = new Date().getHours();
    return hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening';
  }
  get todayCalories(): number {
    return this.daily().find((day) => day.date === this.today())?.totals.calories ?? 0;
  }
  get calorieProgress(): number {
    const target = this.activeGoal?.dailyCalorieTarget ?? 2000;
    return Math.min(100, Math.round((this.todayCalories / target) * 100));
  }
  get trendPeak(): number {
    return Math.max(...this.trends().map((point) => point.calories), 1);
  }
  get averageCalories(): number {
    const points = this.trends();
    return points.length
      ? Math.round(points.reduce((sum, point) => sum + point.calories, 0) / points.length)
      : 0;
  }
  get highestCalorieDay(): TrendPoint | null {
    return this.trends().reduce<TrendPoint | null>(
      (highest, point) => (!highest || point.calories > highest.calories ? point : highest),
      null,
    );
  }
  get calorieLinePoints(): string {
    const points = this.trends();
    if (!points.length) return '';
    const width = points.length === 1 ? 50 : 100 / (points.length - 1);
    return points
      .map(
        (point, index) =>
          `${points.length === 1 ? 50 : index * width},${96 - (point.calories / this.trendPeak) * 82}`,
      )
      .join(' ');
  }
  get macroMetrics() {
    return [
      { key: 'proteinG', label: 'Protein', unit: 'g', color: '#6d8e56' },
      { key: 'carbohydratesG', label: 'Carbohydrates', unit: 'g', color: '#c18b4a' },
      { key: 'fatG', label: 'Fat', unit: 'g', color: '#b56b5b' },
      { key: 'fiberG', label: 'Fiber', unit: 'g', color: '#7a789c' },
      { key: 'sugarG', label: 'Sugar', unit: 'g', color: '#b47791' },
    ] as const;
  }
  get microMetrics() {
    return [
      { key: 'sodiumMg', label: 'Sodium', unit: 'mg', color: '#668e9d' },
      { key: 'calciumMg', label: 'Calcium', unit: 'mg', color: '#8b9b55' },
      { key: 'ironMg', label: 'Iron', unit: 'mg', color: '#a96655' },
      { key: 'magnesiumMg', label: 'Magnesium', unit: 'mg', color: '#8c76a5' },
      { key: 'potassiumMg', label: 'Potassium', unit: 'mg', color: '#bd8650' },
      { key: 'zincMg', label: 'Zinc', unit: 'mg', color: '#5f8e86' },
      { key: 'vitaminAMcg', label: 'Vitamin A', unit: 'mcg', color: '#b77b50' },
      { key: 'vitaminB12Mcg', label: 'Vitamin B12', unit: 'mcg', color: '#786fa0' },
      { key: 'vitaminCMg', label: 'Vitamin C', unit: 'mg', color: '#c2775e' },
      { key: 'vitaminDMcg', label: 'Vitamin D', unit: 'mcg', color: '#6b8c9a' },
      { key: 'vitaminEMg', label: 'Vitamin E', unit: 'mg', color: '#929d57' },
      { key: 'vitaminKMcg', label: 'Vitamin K', unit: 'mcg', color: '#9b6f89' },
    ] as const;
  }
  metricValue(point: TrendPoint, key: string): number {
    return Number(point[key as keyof TrendPoint]) || 0;
  }
  metricPeak(key: string): number {
    return Math.max(...this.trends().map((point) => this.metricValue(point, key)), 1);
  }
  metricLinePoints(key: string): string {
    const points = this.trends();
    if (!points.length) return '';
    const width = points.length === 1 ? 50 : 100 / (points.length - 1);
    const peak = this.metricPeak(key);
    return points
      .map(
        (point, index) =>
          `${points.length === 1 ? 50 : index * width},${96 - (this.metricValue(point, key) / peak) * 82}`,
      )
      .join(' ');
  }

  openNewGoal(): void {
    this.editingGoal.set(null);
    this.showGoalEditor.set(true);
  }
  editGoal(): void {
    this.editingGoal.set(this.activeGoal);
    this.showGoalEditor.set(true);
  }
  openNewFood(): void {
    this.editingEntry.set(null);
    this.showFoodEditor.set(true);
  }
  editEntry(entry: FoodEntry): void {
    this.editingEntry.set(entry);
    this.showFoodEditor.set(true);
  }
  deleteEntry(entry: FoodEntry): void {
    if (!window.confirm(`Delete ${entry.foodName}?`)) return;
    this.foodApi.remove(entry.id).subscribe({
      next: () => {
        this.message.set('Entry removed.');
        this.loadHealthData();
      },
      error: () => this.message.set('We could not remove that entry.'),
    });
  }
  onGoalSaved(): void {
    this.showGoalEditor.set(false);
    this.message.set('Your goal is saved.');
    this.loadHealthData();
  }
  onFoodSaved(): void {
    this.showFoodEditor.set(false);
    this.showImageReview.set(false);
    this.message.set('Your food record is saved.');
    this.loadHealthData();
  }
  onPdfImported(result: ImportFoodEntriesResponse): void {
    this.showPdfImport.set(false);
    this.message.set(
      `${result.importedCount} entr${result.importedCount === 1 ? 'y' : 'ies'} imported${result.skippedCount ? `, ${result.skippedCount} row${result.skippedCount === 1 ? '' : 's'} skipped.` : '.'}`,
    );
    this.loadHealthData();
  }
  closeChat(): void {
    this.chatOpen.set(false);
    this.chatExpanded.set(false);
  }

  private loadHealthData(): void {
    const from = new Date(Date.now() - 6 * 86400000).toISOString().slice(0, 10);
    const to = this.today();
    this.goalApi.getAll().subscribe({ next: (goals) => this.goals.set(goals.items) });
    this.foodApi
      .list({ from, to, pageSize: 20 })
      .subscribe({ next: (entries) => this.entries.set(entries.items) });
    this.reportApi.daily(from, to).subscribe({ next: (daily) => this.daily.set(daily.items) });
    this.reportApi.trends(from, to).subscribe({ next: (trends) => this.trends.set(trends.items) });
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private fillForm(user: User): void {
    this.form.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      password: '',
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.message.set('');
    const value = this.form.getRawValue();
    this.api.update({ ...value, password: value.password || undefined }).subscribe({
      next: (user) => {
        this.user.set(user);
        this.fillForm(user);
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
      error: () => this.message.set('We could not deactivate your account.'),
    });
  }
}
