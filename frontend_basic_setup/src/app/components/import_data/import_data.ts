import {Component, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {importDataService} from './import_data.service';
import {TransactionService} from 'src/app/components/transaction-list/transaction.service';
import {RefreshService} from "../../data/refresh-service";
import {CategorySummaryService} from "../type_pie_chart/category_summary.service";

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
    private refreshService = inject(RefreshService);
    private categorySummaryService = inject(CategorySummaryService);
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
                this.categorySummaryService.triggerRefresh();

            },
            error: err => {
                console.error('Import failed', err)
                this.uploadStatus.set('error')
            }
        });
    
    }

    
}


