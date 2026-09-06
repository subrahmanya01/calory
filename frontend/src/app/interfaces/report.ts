export interface NutritionTotals {
  calories: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  fiberG: number;
  entryCount: number;
}

export interface DailyNutrition {
  date: string;
  totals: NutritionTotals;
}

export interface TrendPoint {
  date: string;
  calories: number;
  proteinG: number;
  carbohydratesG: number;
  fatG: number;
  entryCount: number;
}
