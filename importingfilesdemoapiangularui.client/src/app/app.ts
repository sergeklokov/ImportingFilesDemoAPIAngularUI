import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App {
  protected readonly activeTab = signal<'weather' | 'import'>('weather');
  protected readonly title = signal('ImportingFilesDemoAPIAngularUI');

  protected switchTab(tab: 'weather' | 'import') {
    this.activeTab.set(tab);
  }
}

