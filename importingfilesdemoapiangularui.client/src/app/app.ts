import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  protected readonly activeTab = signal<'weather' | 'import' | 'bulk-import' | 'parallel-bulk-import'>('weather');
  protected readonly title = signal('Importing Files - Demo API with Angular UI');

  protected switchTab(tab: 'weather' | 'import' | 'bulk-import' | 'parallel-bulk-import') {
    this.activeTab.set(tab);
  }
}

