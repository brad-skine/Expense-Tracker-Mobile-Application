import {Component, computed, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {TransactionService} from '../transaction-list/transaction.service';
import {NgxEchartsDirective, provideEchartsCore} from 'ngx-echarts';
import {toSignal} from '@angular/core/rxjs-interop';

import * as echarts from 'echarts/core';
import {BarChart, PieChart} from 'echarts/charts';
import {
    DatasetComponent,
    GridComponent,
    LegendComponent,
    TitleComponent,
    TooltipComponent,
    TransformComponent,
} from 'echarts/components';
import {CanvasRenderer} from 'echarts/renderers';
import {LabelLayout, UniversalTransition} from 'echarts/features';
import {CategorySummaryService} from './category_summary.service';

echarts.use([
    PieChart, BarChart,
    TitleComponent, TooltipComponent, DatasetComponent,
    TransformComponent, LegendComponent, GridComponent,
    CanvasRenderer, LabelLayout, UniversalTransition,
]);

export type ChartType = 'pie' | 'bar';

@Component({
    selector: 'app-category-chart',
    standalone: true,
    imports: [CommonModule, NgxEchartsDirective],
    templateUrl: './category_pie_chart.html',
    styleUrls: ['./category_summary.scss'],
    providers: [provideEchartsCore({echarts})],
})
export class CategoryPieChart {
    private categorySummaryService = inject(CategorySummaryService);
    private transactionService = inject(TransactionService);

    selectedCategory = signal<string | null>(null);
    chartType = signal<ChartType>('pie');

    readonly CATEGORY_COLORS = [
        '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de',
        '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#d14a61',
        '#675bba', '#e0a62e', '#27727b', '#c4ccd3', '#b5495b'
    ];

    categorySummaries = toSignal(
        this.categorySummaryService.getAllCategorySummaries(),
        {initialValue: []}
    );

    allTransactions = toSignal(
        this.transactionService.getAllTransactions(),
        {initialValue: []}
    );

    filteredTransactions = computed(() => {
        const category = this.selectedCategory();
        if (!category) return [];
        return this.allTransactions()
            .filter(t => t.category === category)
            .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
    });

    filteredTotal = computed(() => {
        return this.filteredTransactions()
            .reduce((sum, t) => sum + Math.abs(t.amount), 0);
    });

    // Used only for initial render by ngx-echarts [options] binding

    private getDataset() {
        const sorted = [...this.categorySummaries()]
            .sort((a, b) => Number(b.total) - Number(a.total));
        return {
            dimensions: ['category', 'total'],
            source: sorted.map(d => [d.category, Number(d.total)])
        };
    }
 initOptions = computed(() => {
    const data = this.categorySummaries();
    if (!data.length) return {};
    return this.chartType() === 'pie' ? this.getPieOption() : this.getBarOption();
});

mergeOptions = computed(() => {
    const data = this.categorySummaries();
    if (!data.length) return {};
    return this.chartType() === 'pie' ? this.getPieOption() : this.getBarOption();
});

    private getPieOption() {
        return {
            dataset: [this.getDataset()],
            tooltip: {
                trigger: 'item',
                confine: true,
                formatter: (p: any) => {
                    const val = Number(p.value[1]).toLocaleString('en-NZ', {
                        style: 'currency', currency: 'NZD', minimumFractionDigits: 2
                    });
                    return `<strong>${p.value[0]}</strong><br/>${val}`;
                }
            },
            legend: {show: false},
            series: [{
                id: 'categoryChart',
                type: 'pie',
                radius: ['35%', '65%'],
                center: ['50%', '45%'],
                colorBy: 'data',
                color: this.CATEGORY_COLORS,
                encode: {itemName: 'category', value: 'total'},
                universalTransition: true,
                animationDurationUpdate: 800,
                label: {show: false},
                labelLine: {show: false},
                itemStyle: {borderRadius: 6, borderColor: 'rgba(0,0,0,0.3)', borderWidth: 1},
            }]
        };
    }

    private getBarOption() {
        return {
            dataset: [this.getDataset()],
            tooltip: {
                trigger: 'axis',
                confine: true,
                axisPointer: {type: 'none'},
                formatter: (params: any) => {
                    const p = params[0];
                    const val = Number(p.value[1]).toLocaleString('en-NZ', {
                        style: 'currency', currency: 'NZD', minimumFractionDigits: 2
                    });
                    return `<strong>${p.value[0]}</strong><br/>${val}`;
                }
            },
            legend: {show: false},
            grid: {left: 90, right: 16, top: 16, bottom: 16},
            xAxis: {
                type: 'value',
                axisLabel: {
                    color: '#a0aec0',
                    fontSize: 10,
                    formatter: (v: number) => `$${(v / 1000).toFixed(0)}k`
                },
                splitLine: {lineStyle: {color: 'rgba(255,255,255,0.06)'}},
            },
            yAxis: {
                type: 'category',
                axisLabel: {color: '#cbd5f5', fontSize: 11},
                axisTick: {show: false},
                axisLine: {show: false},
            },
            series: [{
                id: 'categoryChart',
                type: 'bar',
                colorBy: 'data',
                color: this.CATEGORY_COLORS,
                encode: {x: 'total', y: 'category'},
                universalTransition: true,
                animationDurationUpdate: 800,
                barMaxWidth: 28,
                label: {show: false},
                itemStyle: {borderRadius: [0, 6, 6, 0]},
            }]
        };
    }

    toggleChart() {
        this.selectedCategory.set(null);
        this.chartType.update(t => t === 'pie' ? 'bar' : 'pie');
    }

    onChartClick(event: any) {
        if (event.name) this.selectedCategory.set(event.name);
    }

    goBack() {
        this.selectedCategory.set(null);
    }
}
