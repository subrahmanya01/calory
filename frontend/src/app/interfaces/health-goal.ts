export interface HealthGoal {
  id: string;
  dailyCalorieTarget: number;
  proteinTarget: number;
  carbTarget: number;
  fatTarget: number;
  weightTarget: number;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface HealthGoalRequest {
  dailyCalorieTarget: number;
  proteinTarget: number;
  carbTarget: number;
  fatTarget: number;
  weightTarget: number;
  startDate: string;
  endDate?: string | null;
}
