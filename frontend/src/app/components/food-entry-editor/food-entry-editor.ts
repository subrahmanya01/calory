import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FoodEntry, FoodEntryRequest, MealType } from '../../interfaces/food-entry';
import { FoodEntryApi } from '../../services/food-entry-api';
import { Spinner } from '../spinner/spinner';

@Component({
  selector: 'app-food-entry-editor',
  imports: [ReactiveFormsModule, Spinner],
  templateUrl: './food-entry-editor.html',
  styleUrl: './food-entry-editor.css',
})
export class FoodEntryEditor {
  @Input() entry: FoodEntry | null = null;
  @Output() saved = new EventEmitter<FoodEntry>();
  @Output() closed = new EventEmitter<void>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(FoodEntryApi);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly meals: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];
  readonly form = this.formBuilder.nonNullable.group({
    mealType: ['Snack' as MealType, Validators.required],
    foodName: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(0.01)]],
    unit: ['serving', Validators.required],
    consumedAt: [this.localDateTime(), Validators.required],
    notes: [''],
    calories: [0, [Validators.required, Validators.min(0)]],
    proteinG: [0, Validators.min(0)],
    carbohydratesG: [0, Validators.min(0)],
    fatG: [0, Validators.min(0)],
    fiberG: [0, Validators.min(0)],
  });

  ngOnChanges(): void {
    if (!this.entry) return;
    this.form.patchValue({
      mealType: this.entry.mealType,
      foodName: this.entry.foodName,
      quantity: this.entry.quantity,
      unit: this.entry.unit,
      consumedAt: this.entry.consumedAt.slice(0, 16),
      notes: this.entry.notes ?? '',
      calories: this.entry.nutrition.calories,
      proteinG: this.entry.nutrition.proteinG,
      carbohydratesG: this.entry.nutrition.carbohydratesG,
      fatG: this.entry.nutrition.fatG,
      fiberG: this.entry.nutrition.fiberG,
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
    const request: FoodEntryRequest = {
      mealType: value.mealType,
      foodName: value.foodName,
      quantity: value.quantity,
      unit: value.unit,
      consumedAt: new Date(value.consumedAt).toISOString(),
      source: this.entry?.source ?? 'Manual',
      notes: value.notes || null,
      nutrition: this.emptyNutrition(
        value.calories,
        value.proteinG,
        value.carbohydratesG,
        value.fatG,
        value.fiberG,
      ),
    };
    const call = this.entry ? this.api.update(this.entry.id, request) : this.api.create(request);
    call.subscribe({
      next: (entry) => {
        this.saving.set(false);
        this.saved.emit(entry);
      },
      error: () => {
        this.saving.set(false);
        this.error.set('We could not save this food entry.');
      },
    });
  }

  private emptyNutrition(
    calories: number,
    proteinG: number,
    carbohydratesG: number,
    fatG: number,
    fiberG: number,
  ) {
    return {
      calories,
      proteinG,
      carbohydratesG,
      fatG,
      fiberG,
      sugarG: 0,
      sodiumMg: 0,
      calciumMg: 0,
      ironMg: 0,
      magnesiumMg: 0,
      potassiumMg: 0,
      zincMg: 0,
      vitaminAMcg: 0,
      vitaminB1Mg: 0,
      vitaminB2Mg: 0,
      vitaminB3Mg: 0,
      vitaminB6Mg: 0,
      vitaminB12Mcg: 0,
      vitaminCMg: 0,
      vitaminDMcg: 0,
      vitaminEMg: 0,
      vitaminKMcg: 0,
    };
  }
  private localDateTime(): string {
    const now = new Date();
    return new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  }
}
