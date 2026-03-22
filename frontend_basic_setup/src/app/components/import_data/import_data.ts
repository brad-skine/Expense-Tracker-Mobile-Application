import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { importDataService } from './import_data.service';
import { TransactionService } from 'src/app/components/transaction-list/transaction.service';
import { monthlySalesService } from 'src/app/components/monthly-chart/monthly_sales.service';
import { TypeSummaryService } from '../type_pie_chart/type_summary.service';
import {RefreshService} from "../../data/refresh-service";
@Component({
  selector: 'app-import_button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './import_data.html',
  styleUrls: ['./import_data.scss'],
}) 
export class ImportButtonComponent{
    private importService = inject(importDataService); 
    private transactionService = inject(TransactionService);
    private monthlySalesService = inject(monthlySalesService)
    private typeSummaryService = inject(TypeSummaryService);
    private refreshService = inject(RefreshService);
    // import_result$: Observable<string> = this.importService.importData();

    uploadStatus = signal<'idle' | 'success' | 'error'>('idle');

    onFileSelect(event: Event): void {
        // this.importService.importData();
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;


        this.uploadStatus.set('idle'); // reset before each attempt
        this.importService.importData(file).subscribe({
            next: () => {
                console.log('Import success')
                this.uploadStatus.set('success');
                this.transactionService.triggerRefresh();
                this.refreshService.triggerRefresh();
                // this.monthlySalesService.triggerRefresh();
                this.typeSummaryService.triggerRefresh();

            },
            error: err => {
                console.error('Import failed', err)
                this.uploadStatus.set('error')
            }
        });
    
    }

    
}


