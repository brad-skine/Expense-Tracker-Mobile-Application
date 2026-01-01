import { Component, computed, inject } from '@angular/core';
import { monthlySalesService } from 'src/app/services/monthly_sales.service';
import { CommonModule} from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore} from 'ngx-echarts';
import { toSignal } from '@angular/core/rxjs-interop';

import * as echarts from 'echarts/core';
// import necessary echarts components
import { BarChart } from 'echarts/charts';
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  LegendComponent,
  DataZoomComponent,
  ToolboxComponent
} from 'echarts/components';

import { CanvasRenderer } from 'echarts/renderers';
echarts.use([
  BarChart,
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  CanvasRenderer,
  LegendComponent,
  DataZoomComponent,
  ToolboxComponent
]);

@Component({
  selector: 'app-monthly-chart',
  standalone: true,
  imports: [CommonModule, NgxEchartsDirective],
  templateUrl: './monthly-chart.html',
  styleUrls: ['./monthly-chart.scss'],
  providers: [provideEchartsCore({echarts})]
})

export class MonthlyChartComponent{

  protected monthlySalesService = inject(monthlySalesService); 
  // loading = false;
  error: string | null = null; // so can check if error then display it

   monthlySales = toSignal(this.monthlySalesService.getAllMonthlySales(
    ), {initialValue: null});

    
  chartOptions = computed(() => {
      const data = this.monthlySales();
      if (!data || !data.length) return null;

      return {
        textStyle: {
          fontSize: '1rem'
        },
        title: {
          text: 'Monthly Income\nvs Expense',
          left: 'center',
          textStyle: {
            color: '#cbd5f5',
            fontSize: '1rem'
          },

        },
        tooltip: { trigger: 'axis', confine: true},
        legend: {
          data: ['Income', 'Expense'],
          top: 60
        },
        grid: {
          left: 40,
          right: 30,
          bottom: 100,
          top: 100
        },
        xAxis: {
          type: 'category',
          data: data.map(s => `${s.month } ${s.year}`),
          axisLabel: { rotate: 45 }
        },
        yAxis: { 
          type: 'value' 
        },

         dataZoom: [
        {
          type: 'slider',
          show: true,
          start: 4,
          end: 100,
          handleSize: 8
        },
        {
          type: 'inside',
          start: 94,
          end: 100
        }
      ],
      toolbox: {

        show: true,
        orient: 'vertical',
        bottom: '45%',
        right: -10,
        // top: '50%',
        feature: {
            // Enable the built-in restore button
            
            restore: {
                show: true,
                title: 'Reset Zoom' 
            },

            dataView: {
                show: true,
                readOnly: false
            },
            saveAsImage: {
                show: true
            }
        }, //iconStyle: {
            // color: 'red'
        //}
    },

        series: [
          {
            name: 'Income',
            type: 'bar',
            data: data.map(s => s.income),
            itemStyle: { color: '#4caf50' }
          },
          {
            name: 'Expense',
            type: 'bar',
            data: data.map(s => s.expense),
            itemStyle: { color: '#f44336' }
          }
        ]
      };
    });
}





