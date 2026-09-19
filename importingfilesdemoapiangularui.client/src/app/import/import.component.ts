import { HttpClient } from '@angular/common/http';
import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-import',
  templateUrl: './import.component.html',
  styleUrl: './import.component.css',
  standalone: false
})
export class ImportComponent {
  protected readonly sourceType = signal('Parking');
  protected readonly createdBy = signal('system');
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly message = signal('');
  protected readonly messageType = signal<'success' | 'error' | ''>('');

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

    const formData = new FormData();
    formData.append('file', file);
    formData.append('sourceType', this.sourceType());
    formData.append('createdBy', this.createdBy());

    this.http.post<any>('/api/import/file', formData).subscribe({
      next: (result) => {
        this.isLoading.set(false);
        this.setMessage(
          `Successfully imported ${result.imported} records from ${result.file}`,
          'success'
        );
        // Reset form
        this.selectedFile.set(null);
        const fileInput = document.getElementById('fileInput') as HTMLInputElement;
        if (fileInput) fileInput.value = '';
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = error.error?.detail || error.message || 'Import failed';
        this.setMessage(errorMessage, 'error');
        console.error('Import error:', error);
      }
    });
  }

  private setMessage(msg: string, type: 'success' | 'error') {
    this.message.set(msg);
    this.messageType.set(type);
  }
}

