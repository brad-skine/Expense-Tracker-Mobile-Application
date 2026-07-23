import {Component, EventEmitter, inject, Input, OnInit, Output, Signal, signal} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { TransactionModel } from '../../models/transaction.model';
import { CategorySummaryService } from '../type_pie_chart/category_summary.service';
import { RefreshService } from '../../data/refresh-service';
import {CategoryModel, TransactionService} from "../transaction-list/transaction.service";
import {Observable} from "rxjs";

@Component({
    selector: 'app-transaction-form',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './transaction-form.html',
    styleUrl: './transaction-form.scss',
})
export class TransactionFormComponent implements OnInit {
    private txService = inject(TransactionService);
    private categorySummaryService = inject(CategorySummaryService);
    private refreshService = inject(RefreshService);

    /** null = adding a new transaction, otherwise editing this one */
    @Input() transaction: TransactionModel | null = null;
    @Output() closed = new EventEmitter<void>();

    categories:Signal<CategoryModel[]> = toSignal(this.txService.categories$, { initialValue: []});

    // form state
    date = signal(new Date().toISOString().slice(0, 10));
    description = signal('');
    amount = signal<number | null>(null);
    category = signal('');           // '' = auto-classify on create
    isExpense = signal(true);

    saving = signal(false);
    error = signal('');
    confirmingDelete = signal(false);

    get isEdit() { return this.transaction !== null; }

    ngOnInit() {
        const tx = this.transaction;
        if (tx) {
            this.date.set(tx.date.slice(0, 10));
            this.description.set(tx.description);
            this.amount.set(Math.abs(tx.amount));
            this.category.set(tx.category);
            this.isExpense.set(tx.amount < 0);
        }
    }

    save() {
        const amt = this.amount();
        if (!this.description().trim() || amt === null || amt <= 0) {
            this.error.set('Description and a positive amount are required');
            return;
        }

        const payload = {
            date: this.date(),
            description: this.description().trim(),
            amount: this.isExpense() ? -Math.abs(amt) : Math.abs(amt),
            category: this.category() || null,
        };

        this.saving.set(true);
        this.error.set('');

        const request:Observable<unknown> = this.isEdit
            ? this.txService.updateTransaction(this.transaction!.id, {
                ...payload,
                category: payload.category ?? 'Other',
              })
            : this.txService.addTransaction(payload);

        request.subscribe({
            next: () => this.finish(),
            error: () => {
                this.saving.set(false);
                this.error.set('Save failed — try again');
            },
        });
    }

    delete() {
        if (!this.confirmingDelete()) {
            this.confirmingDelete.set(true);
            return;
        }
        this.saving.set(true);
        this.txService.deleteTransaction(this.transaction!.id).subscribe({
            next: () => this.finish(),
            error: () => {
                this.saving.set(false);
                this.error.set('Delete failed — try again');
            },
        });
    }

    private finish() {
        // refresh every cached stream so list + charts + budgets all update
        this.txService.triggerRefresh();
        this.categorySummaryService.triggerRefresh();
        this.refreshService.triggerRefresh();
        this.closed.emit();
    }

    cancel() {
        this.closed.emit();
    }
}
