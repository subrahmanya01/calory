import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';
import { GoalEditor } from '../../components/goal-editor/goal-editor';
import { HealthGoal } from '../../interfaces/health-goal';
import { HealthGoalApi } from '../../services/health-goal-api';

@Component({
  selector: 'app-goal-history',
  imports: [CommonModule, RouterLink, Header, Footer, GoalEditor],
  templateUrl: './goal-history.html',
  styleUrl: './goal-history.css',
})
export class GoalHistory {
  readonly goals = signal<HealthGoal[]>([]);
  readonly loading = signal(true);
  readonly message = signal('');
  readonly showEditor = signal(false);
  readonly editingGoal = signal<HealthGoal | null>(null);

  constructor(private readonly api: HealthGoalApi) { this.load(); }

  isCurrent(goal: HealthGoal): boolean {
    const today = this.today();
    return goal.startDate <= today && (!goal.endDate || goal.endDate >= today);
  }

  openNew(): void { this.editingGoal.set(null); this.showEditor.set(true); }
  edit(goal: HealthGoal): void { this.editingGoal.set(goal); this.showEditor.set(true); }
  saved(): void { this.showEditor.set(false); this.message.set('Goal saved.'); this.load(); }
  private load(): void {
    this.api.getAll(1, 100).subscribe({
      next: (result) => { this.goals.set(result.items); this.loading.set(false); },
      error: () => { this.message.set('We could not load your goal history.'); this.loading.set(false); },
    });
  }
  private today(): string { return new Date().toISOString().slice(0, 10); }

}
