export type MealType = 'Breakfast' | 'Lunch' | 'Dinner' | 'Snack';
export type FoodEntrySource = 'Manual' | 'Ai' | 'Database';

export interface FoodNutrition {
  calories: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  sugarG: number;
  sodiumMg: number;
  calciumMg: number;
  ironMg: number;
  magnesiumMg: number;
  potassiumMg: number;
  zincMg: number;
  vitaminAMcg: number;
  vitaminB1Mg: number;
  vitaminB2Mg: number;
  vitaminB3Mg: number;
  vitaminB6Mg: number;
  vitaminB12Mcg: number;
  vitaminCMg: number;
  vitaminDMcg: number;
  vitaminEMg: number;
  vitaminKMcg: number;
}

export interface FoodEntry {
  id: string;
  mealType: MealType;
  foodName: string;
  quantity: number;
  unit: string;
  consumedAt: string;
  source: FoodEntrySource;
  notes: string | null;
  nutrition: FoodNutrition;
}

export interface FoodEntryRequest {
  mealType: MealType;
  foodName: string;
  quantity: number;
  unit: string;
  consumedAt: string;
  source: FoodEntrySource;
  notes?: string | null;
  nutrition: FoodNutrition;
}

export interface FoodAnalysisResponse {
  mealType: string;
  foodName: string;
  quantity: number;
  unit: string;
  nutrition: FoodNutrition;
}

export interface FoodEntryQuery {
  from?: string;
  to?: string;
  mealType?: string;
  minCalories?: number;
  maxCalories?: number;
  page?: number;
  pageSize?: number;
}

export interface ImportSkippedRow {
  rowNumber: number;
  content: string;
  reason: string;
}
export interface ImportFoodEntriesResponse {
  importedCount: number;
  skippedCount: number;
  importedEntries: FoodEntry[];
  skippedRows: ImportSkippedRow[];
}
