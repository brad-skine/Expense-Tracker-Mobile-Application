import {Component, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CategoryPieChart} from '../../components/type_pie_chart/category_pie_chart';
import {MonthlyChartComponent} from '../../components/monthly-chart/monthly-chart';
import {D3TrendChartComponent} from '../../components/d3-trend-chart/d3-trend-chart';

export type HomeTab = 'category' | 'monthly' | 'trends';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, CategoryPieChart, MonthlyChartComponent, D3TrendChartComponent],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomeComponent {
  activeTab = signal<HomeTab>('category');

  tabs: { key: HomeTab; label: string }[] = [
    { key: 'category', label: 'Spending' },
    { key: 'monthly',  label: 'Monthly' },
    { key: 'trends',   label: 'Trends' },
    // add more here later
  ];

  setTab(key: HomeTab) {
    this.activeTab.set(key);
  }
}
