import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
@Injectable({providedIn: 'root',})
  export class importDataService {
    private apiUrl = environment.apiUrl + "/api/import/transactions";
    private http = inject(HttpClient)
  
    importData(csvData: File) {
      const formData = new FormData();
      formData.append('file', csvData);
        return this.http.post<void>(this.apiUrl, formData);
    }

  }