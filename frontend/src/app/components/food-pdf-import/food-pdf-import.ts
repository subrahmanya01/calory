import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FoodEntryApi, ImportFoodEntriesResponse } from '../../services/food-entry-api';
import { Spinner } from '../spinner/spinner';

@Component({
  selector: 'app-food-pdf-import',
  imports: [Spinner],
  templateUrl: './food-pdf-import.html',
  styleUrl: './food-pdf-import.css',
})
export class FoodPdfImport {
  @Output() imported = new EventEmitter<ImportFoodEntriesResponse>();
  @Output() closed = new EventEmitter<void>();

  private readonly api = inject(FoodEntryApi);
  readonly uploading = signal(false);
  readonly error = signal('');
  readonly selectedFile = signal<File | null>(null);

  choose(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.error.set('');
    if (file && (!file.name.toLowerCase().endsWith('.pdf') || file.size > 10 * 1024 * 1024)) {
      this.selectedFile.set(null);
      this.error.set('Choose a PDF file no larger than 10 MB.');
      return;
    }
    this.selectedFile.set(file);
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file || this.uploading()) return;
    this.uploading.set(true);
    this.error.set('');
    this.api.importPdf(file).subscribe({
      next: (result) => { this.uploading.set(false); this.imported.emit(result); },
      error: () => { this.uploading.set(false); this.error.set('We could not read that PDF. Check that it contains a tabular food diary.'); },
    });
  }
}