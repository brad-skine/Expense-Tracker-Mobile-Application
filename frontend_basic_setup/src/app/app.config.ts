/*!
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.dev/license
 */

import {ApplicationConfig} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideEchartsCore} from 'ngx-echarts';
import * as echarts from 'echarts/core';
import {authInterceptor} from "./components/auth/auth-interceptor";


export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    provideEchartsCore({ echarts})
  ],
};
