import { HttpClient } from '@angular/common/http';
import { Component, signal } from '@angular/core';

interface BulkImportResult {
  imported: number;
  file: string;
  executionTimeMs: number;
}

@Component({
  selector: 'app-bulk-import',
  templateUrl: './bulk-import.component.html',
  styleUrl: './bulk-import.component.css',
  standalone: false
})
export class BulkImportComponent {
  protected readonly sourceType = signal('Parking');
  protected readonly createdBy = signal('system');
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly message = signal('');
  protected readonly messageType = signal<'success' | 'error' | ''>('');
  protected readonly executionTimeMs = signal<number | null>(null);

  protected readonly sourceTypes = ['Parking', 'Driving', 'Traffic'];

  constructor(private http: HttpClient) {}

  onSourceTypeChange(value: string) {
    this.sourceType.set(value);
  }

  onCreatedByChange(value: string) {
    this.createdBy.set(value);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile.set(input.files[0]);
    }
  }

  onSubmit() {
    const file = this.selectedFile();
    if (!file) {
      this.setMessage('Please select a file', 'error');
      return;
    }

    this.isLoading.set(true);
    this.message.set('');
    this.executionTimeMs.set(null);

    const formData = new FormData();
    formData.append('file', file);
    formData.append('sourceType', this.sourceType());
    formData.append('createdBy', this.createdBy());

    this.http.post<BulkImportResult>('/api/import/file-bulk', formData).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.executionTimeMs.set(result.executionTimeMs);
        this.setMessage(
          `Successfully imported ${result.imported} records from ${result.file}`,
          'success'
        );
        this.selectedFile.set(null);
        const fileInput = document.getElementById('bulkFileInput') as HTMLInputElement;
        if (fileInput) fileInput.value = '';
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = error.error?.detail || error.message || 'Bulk import failed';
        this.setMessage(errorMessage, 'error');
        console.error('Bulk import error:', error);
      }
    });
  }

  private setMessage(msg: string, type: 'success' | 'error') {
    this.message.set(msg);
    this.messageType.set(type);
  }
}
