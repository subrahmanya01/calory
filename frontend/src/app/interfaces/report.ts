export interface NutritionTotals {
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
  entryCount: number;
}
