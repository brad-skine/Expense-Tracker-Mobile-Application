import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TypeSummaryService } from './type_summary.service';
import { NgxEchartsDirective, provideEchartsCore} from 'ngx-echarts';
import { toSignal } from '@angular/core/rxjs-interop';

import * as echarts from 'echarts/core';
import { PieChart } from 'echarts/charts';

import {
  TitleComponent,
  TooltipComponent,
  DatasetComponent,
  TransformComponent,
  LegendComponent
} from 'echarts/components';

import { CanvasRenderer } from 'echarts/renderers';
import { LabelLayout } from 'echarts/features';
echarts.use([
  PieChart,
  TitleComponent,
  TooltipComponent,
  DatasetComponent,
  TransformComponent,
  CanvasRenderer,
  LegendComponent,
  LabelLayout
]);

@Component({
  selector: 'app-type-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective],
  templateUrl: './type_pie_chart.html',
  styleUrls: ['./type_pie_chart.scss'],
  providers: [provideEchartsCore({echarts})]
})


export class TypePieChartComponent  {
  private typeSummaryService = inject(TypeSummaryService); 
  error: string | null = null; // so can check if error then display it

   typeSummaries = toSignal(this.typeSummaryService.getAllTypeSummaries(
    ), {initialValue: []});

  chartOptions = computed(() => {
    const data = this.typeSummaries();
    console.log(data);

    if (!data.length) return null;

    return {
        textStyle: {
        fontSize: 14
        },
        title: {
        text: 'Transaction totals by transaction type',
        left: 'center',
            textStyle: {
                color: '#cbd5f5',
                fontSize: 14
            }
        },
        tooltip: {
          trigger: 'item',
          confine: true,
          formatter: (params: any) => {
            const value = Number(params.value).toLocaleString('en-NZ', {
              style: 'currency',
              currency: 'NZD',
              minimumFractionDigits: 2
            });
            return `
              <strong>${params.name}</strong><br/>
              ${value} (${params.percent}%)
            `;
          }

        },

        legend: {
          show: true,
          bottom: 120,
          textStyle: {
            color: '#cbd5f5',
            fontSize: 12
          }
        },

        series: [
        {
            colorBy: 'data',
            type: 'pie',
            containLabel: true,
            minAngle: 15,
            radius: '65%',
            center: ['50%', '35%'],
            data: data.map(d => ({
            name: d.transactionType,
             value: Number(d.total)
            })),

            // roseType: 'area',
            label: {
                show: false
            },
            labelLine: {
               show: false
            },
            itemStyle: {
                shadowBlur: 200,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
            },
            animationType: 'scale',
            animationEasing: 'elasticOut',
            animationDelay: function () {
                return Math.random() * 200;
            }
        }
        ],
    };
})
}