import { Component } from '@angular/core';
import { TransactionListComponent } from '../components/transaction-list/transaction-list';
import { MonthlyChartComponent } from "../components/monthly-chart/monthly-chart";
import { ImportButtonComponent } from "../components/import_data/import_data";
import { TypePieChartComponent } from "../components/type_pie_chart/type_pie_chart";

@Component({
  selector: 'app-layout',
  imports: [TransactionListComponent, MonthlyChartComponent, ImportButtonComponent, TypePieChartComponent],
  templateUrl: './layout.html',
  styleUrl: `./layout.scss`,
})
export class LayoutComponent {

}
