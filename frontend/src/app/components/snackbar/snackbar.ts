import { Component, signal } from '@angular/core';

export interface Snack {
  message: string;
  tone: 'success' | 'error';
}

@Component({
  selector: 'app-snackbar',
  imports: [],
  templateUrl: './snackbar.html',
  styleUrl: './snackbar.css',
})
export class Snackbar {
  readonly snack = signal<Snack | null>(null);

  show(message: string, tone: Snack['tone'] = 'success'): void {
    this.snack.set({ message, tone });
    window.setTimeout(() => this.snack.set(null), 3500);
  }

}
