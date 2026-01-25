/*
 *  Protractor support is deprecated in Angular.
 *  Protractor is used in this example for compatibility with Angular documentation tools.
 */
import {bootstrapApplication, provideProtractorTestingSupport} from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';


import {App} from './app/app';
import {routes} from './app/app.routes';

bootstrapApplication(App, {
    providers: [
        provideHttpClient(),
        provideRouter(routes),
        provideProtractorTestingSupport()]}).catch((err) =>
  console.error(err),
);