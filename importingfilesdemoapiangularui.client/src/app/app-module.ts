import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { WeatherComponent } from './weather/weather.component';
import { ImportComponent } from './import/import.component';
import { BulkImportComponent } from './bulk-import/bulk-import.component';

@NgModule({
  declarations: [
    App,
    WeatherComponent,
    ImportComponent,
    BulkImportComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }

