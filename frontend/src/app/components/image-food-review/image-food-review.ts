import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  FoodAnalysisResponse,
  FoodEntry,
  FoodEntryRequest,
  FoodNutrition,
  MealType,
} from '../../interfaces/food-entry';
import { FoodAnalysisApi } from '../../services/food-analysis-api';
import { FoodEntryApi } from '../../services/food-entry-api';
import { Spinner } from '../spinner/spinner';

@Component({
  selector: 'app-image-food-review',
  imports: [ReactiveFormsModule, Spinner],
  templateUrl: './image-food-review.html',
  styleUrl: './image-food-review.css',
})
export class ImageFoodReview {
  @Output() saved = new EventEmitter<FoodEntry>();
  @Output() closed = new EventEmitter<void>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly analyzer = inject(FoodAnalysisApi);
  private readonly entries = inject(FoodEntryApi);
  readonly analyzing = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly preview = signal<string | null>(null);
  readonly meals: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack'];
  readonly form = this.formBuilder.nonNullable.group({
    mealType: ['Snack' as MealType, Validators.required],
    foodName: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(0.01)]],
    unit: ['serving', Validators.required],
    consumedAt: [this.localDateTime(), Validators.required],
    calories: [0, Validators.min(0)],
    proteinG: [0, Validators.min(0)],
    carbohydratesG: [0, Validators.min(0)],
    fatG: [0, Validators.min(0)],
    fiberG: [0, Validators.min(0)],
  });

  analyze(file: File | null): void {
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.error.set('Choose an image file.');
      return;
    }
    this.error.set('');
    this.analyzing.set(true);
    this.preview.set(URL.createObjectURL(file));
    this.analyzer.analyze(file).subscribe({
      next: (result) => {
        this.fill(result);
        this.analyzing.set(false);
      },
      error: () => {
        this.analyzing.set(false);
        this.error.set('The image could not be analyzed. Check the API key and try again.');
      },
    });
  }

  save(): void {
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
      source: 'Ai',
      nutrition: this.nutrition(value),
      notes: 'Analyzed from an uploaded image.',
    };
    this.entries.create(request).subscribe({
      next: (entry) => {
        this.saving.set(false);
        this.saved.emit(entry);
      },
      error: () => {
        this.saving.set(false);
        this.error.set('We could not save this analyzed entry.');
      },
    });
  }

  private fill(result: FoodAnalysisResponse): void {
    this.form.patchValue({
      mealType: this.meals.includes(result.mealType as MealType)
        ? (result.mealType as MealType)
        : 'Snack',
      foodName: result.foodName,
      quantity: result.quantity,
      unit: result.unit,
      calories: result.nutrition.calories,
      proteinG: result.nutrition.proteinG,
      carbohydratesG: result.nutrition.carbohydratesG,
      fatG: result.nutrition.fatG,
      fiberG: result.nutrition.fiberG,
    });
  }
  private nutrition(value: ReturnType<typeof this.form.getRawValue>): FoodNutrition {
    return {
      calories: value.calories,
      proteinG: value.proteinG,
      carbohydratesG: value.carbohydratesG,
      fatG: value.fatG,
      fiberG: value.fiberG,
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
