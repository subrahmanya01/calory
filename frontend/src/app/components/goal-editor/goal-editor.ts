import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HealthGoal, HealthGoalRequest } from '../../interfaces/health-goal';
import { HealthGoalApi } from '../../services/health-goal-api';
import { Spinner } from '../spinner/spinner';

@Component({
  selector: 'app-goal-editor',
  imports: [ReactiveFormsModule, Spinner],
  templateUrl: './goal-editor.html',
  styleUrl: './goal-editor.css',
})
export class GoalEditor implements OnChanges {
  @Input() goal: HealthGoal | null = null;
  @Output() saved = new EventEmitter<HealthGoal>();
  @Output() closed = new EventEmitter<void>();

  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(HealthGoalApi);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    dailyCalorieTarget: [2000, [Validators.required, Validators.min(1)]],
    proteinTarget: [120, [Validators.required, Validators.min(0)]],
    carbTarget: [220, [Validators.required, Validators.min(0)]],
    fatTarget: [65, [Validators.required, Validators.min(0)]],
    weightTarget: [70, [Validators.required, Validators.min(1)]],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    endDate: [''],
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['goal']) {
      const goal = this.goal;
      this.form.patchValue(goal ? {
        dailyCalorieTarget: goal.dailyCalorieTarget,
        proteinTarget: goal.proteinTarget,
        carbTarget: goal.carbTarget,
        fatTarget: goal.fatTarget,
        weightTarget: goal.weightTarget,
        startDate: goal.startDate,
        endDate: goal.endDate ?? '',
      } : {
        dailyCalorieTarget: 2000, proteinTarget: 120, carbTarget: 220,
        fatTarget: 65, weightTarget: 70, startDate: new Date().toISOString().slice(0, 10), endDate: '',
      });
    }
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.error.set('');
    const value = this.form.getRawValue();
    const request: HealthGoalRequest = { ...value, endDate: value.endDate || null };
    const call = this.goal ? this.api.update(this.goal.id, request) : this.api.create(request);
    call.subscribe({
      next: (goal) => { this.saving.set(false); this.saved.emit(goal); },
      error: () => { this.saving.set(false); this.error.set('We could not save this goal. Please try again.'); },
    });
  }
}
