import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FoodEntry, MealType } from '../../interfaces/food-entry';
import { Header } from '../../components/header/header';
import { Footer } from '../../components/footer/footer';
import { FoodEntryApi } from '../../services/food-entry-api';
import { FoodEntryEditor } from '../../components/food-entry-editor/food-entry-editor';

@Component({
  selector: 'app-my-query',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Header, Footer, FoodEntryEditor],
  templateUrl: './my-query.html',
  styleUrl: './my-query.css',
})
export class MyQuery {
  private readonly formBuilder = inject(FormBuilder);
  readonly entries = signal<FoodEntry[]>([]);
  readonly loading = signal(false);
  readonly message = signal('');
  readonly page = signal(1);
  readonly totalPages = signal(1);
  readonly totalCount = signal(0);
  readonly editingEntry = signal<FoodEntry | null>(null);
  readonly showEditor = signal(false);
  readonly mealTypes: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];
  readonly form = this.formBuilder.nonNullable.group({
    from: [this.daysAgo(30)],
    to: [this.today()],
    mealType: [''],
    minCalories: [''],
    maxCalories: [''],
  });

  constructor(private readonly api: FoodEntryApi) {
    this.search();
  }

  search(page = 1): void {
    const value = this.form.getRawValue();
    this.loading.set(true);
    this.message.set('');
    this.api
      .list({
        from: value.from || undefined,
        to: value.to || undefined,
        mealType: value.mealType || undefined,
        minCalories: value.minCalories === '' ? undefined : Number(value.minCalories),
        maxCalories: value.maxCalories === '' ? undefined : Number(value.maxCalories),
        page,
        pageSize: 10,
      })
      .subscribe({
        next: (result) => {
          this.entries.set(result.items);
          this.page.set(result.page);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.message.set('We could not load those entries.');
        },
      });
  }

  reset(): void {
    this.form.reset({
      from: this.daysAgo(30),
      to: this.today(),
      mealType: '',
      minCalories: '',
      maxCalories: '',
    });
    this.search();
  }

  edit(entry: FoodEntry): void {
    this.editingEntry.set(entry);
    this.showEditor.set(true);
  }
  remove(entry: FoodEntry): void {
    if (!window.confirm(`Delete ${entry.foodName}?`)) return;
    this.api
      .remove(entry.id)
      .subscribe({
        next: () => this.search(this.page()),
        error: () => this.message.set('We could not remove that entry.'),
      });
  }
  saved(): void {
    this.showEditor.set(false);
    this.search(this.page());
  }
  pageNumbers(): number[] {
    return Array.from({ length: this.totalPages() }, (_, index) => index + 1);
  }
  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
  private daysAgo(days: number): string {
    return new Date(Date.now() - days * 86400000).toISOString().slice(0, 10);
  }
}
