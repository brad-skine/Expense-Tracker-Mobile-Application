import {
  Component, ElementRef, OnDestroy, computed, effect,
  inject, signal, viewChild, Signal
} from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import * as d3 from 'd3';
import { TransactionModel } from '../../models/transaction.model';
import { TransactionService } from '../transaction-list/transaction.service';
import { CategoryService } from '../../data/category.service';
import { CategoryIconComponent } from '../category-icon/category-icon';
import { TransactionFormComponent } from '../transaction-form/transaction-form';

type ZoomTier = 'overview' | 'cluster' | 'detail';

interface AggregatedPoint {
  id: string;
  category: string;
  date: Date;
  amount: number;
  radius: number;
  x?: number;
  y?: number;
}

@Component({
  selector: 'app-d3-zoom-explorer',
  standalone: true,
  imports: [CommonModule, CategoryIconComponent, TransactionFormComponent],
  providers: [DecimalPipe, DatePipe],
  templateUrl: './d3-zoom-explorer.html',
  styleUrl: './d3-zoom-explorer.scss',
})
export class D3ZoomExplorerComponent implements OnDestroy {
  private transactionService = inject(TransactionService);
  private categoryService = inject(CategoryService);
  protected Math = Math;

  private chartHost = viewChild<ElementRef<HTMLDivElement>>('chartHost');

  // ---- State ----
  currentTier = signal<ZoomTier>('overview');
  dateRange = signal<string>('');
  showTop500Hint = signal<boolean>(false);
  selectedTransaction = signal<TransactionModel | null>(null);
  editingTransaction = signal<TransactionModel | null>(null);

  // ---- Data ----
  private allTransactions: Signal<TransactionModel[]> = toSignal(
    this.transactionService.getAllTransactions(),
    { initialValue: [] as TransactionModel[] }
  );

  private expenses = computed(() =>
    this.allTransactions().filter(t => t.amount < 0)
  );

  // ---- D3 Fields ----
  private svg?: d3.Selection<SVGSVGElement, unknown, null, undefined>;
  private zoom?: d3.ZoomBehavior<SVGSVGElement, unknown>;
  private resizeObserver?: ResizeObserver;
  private currentTransform = d3.zoomIdentity;
  
  // Scales and state stored to be accessible in zoom handler
  private x0 = d3.scaleTime();
  private y = d3.scaleSqrt();
  private lastData: TransactionModel[] = [];
  private lastIw = 0;
  private lastIh = 0;

  constructor() {
    effect(() => {
      const host = this.chartHost()?.nativeElement;
      const data = this.expenses();
      if (!host || !data.length) return;
      
      // Initial render or data update
      requestAnimationFrame(() => this.initOrUpdate(host, data));
    });

    effect(() => {
      const host = this.chartHost()?.nativeElement;
      if (!host) return;
      this.resizeObserver?.disconnect();
      this.resizeObserver = new ResizeObserver(() => {
        const data = this.expenses();
        if (data.length) this.initOrUpdate(host, data);
      });
      this.resizeObserver.observe(host);
    });
  }

  ngOnDestroy() {
    this.resizeObserver?.disconnect();
  }

  private initOrUpdate(host: HTMLDivElement, data: TransactionModel[]) {
    const width = host.clientWidth;
    const height = host.clientHeight;
    if (!width || !height) return;

    const margin = { top: 20, right: 20, bottom: 30, left: 40 };
    const iw = width - margin.left - margin.right;
    const ih = height - margin.top - margin.bottom;
    
    this.lastData = data;
    this.lastIw = iw;
    this.lastIh = ih;

    if (!this.svg) {
      this.svg = d3.select(host).append('svg')
        .attr('style', 'width: 100%; height: 100%;');
      
      const g = this.svg.append('g')
        .attr('class', 'main-g')
        .attr('transform', `translate(${margin.left},${margin.top})`);

      g.append('g').attr('class', 'grid');
      g.append('g').attr('class', 'x-axis').attr('transform', `translate(0,${ih})`);
      g.append('g').attr('class', 'y-axis');

      g.append('g').attr('class', 'tier-overview');
      g.append('g').attr('class', 'tier-cluster');
      g.append('g').attr('class', 'tier-detail');

      this.zoom = d3.zoom<SVGSVGElement, unknown>()
        .scaleExtent([1, 100])
        .on('zoom', (event) => this.onZoom(event));

      this.svg.call(this.zoom)
        .on('dblclick.zoom', () => {
          this.svg?.transition().duration(500).call(this.zoom!.transform, d3.zoomIdentity);
        });
    }

    this.svg
      .attr('width', width)
      .attr('height', height)
      .attr('viewBox', `0 0 ${width} ${height}`);

    // Update axes group positions on resize
    const g = this.svg.select<SVGGElement>('g.main-g');
    g.select('g.x-axis').attr('transform', `translate(0,${ih})`);

    // Setup initial scales
    const dateExtent = d3.extent(data, d => new Date(d.date)) as [Date, Date];
    this.x0 = d3.scaleTime().domain(dateExtent).range([0, iw]);
    
    // Y scale domain should cover the max monthly aggregate to keep bubbles in view
    const monthlyRollup = d3.rollup(data, 
      v => d3.sum(v, d => Math.abs(d.amount)),
      d => d.category,
      d => d3.timeMonth.floor(new Date(d.date)).getTime()
    );
    let maxAgg = 0;
    monthlyRollup.forEach(dates => dates.forEach(total => { if (total > maxAgg) maxAgg = total; }));
    
    this.y = d3.scaleSqrt().domain([0, maxAgg || 1000]).range([ih, 0]).nice();

    // Draw initial state
    this.updateChart(data, iw, ih);
  }

  private onZoom(event: d3.D3ZoomEvent<SVGSVGElement, unknown>) {
    this.currentTransform = event.transform;
    this.updateChart(this.lastData, this.lastIw, this.lastIh);
  }

  private updateChart(data: TransactionModel[], iw: number, ih: number) {
    if (!this.svg) return;

    const transform = this.currentTransform;
    const zx = transform.rescaleX(this.x0);
    const k = transform.k;

    // Determine tier
    let tier: ZoomTier = 'overview';
    if (k >= 3 && k < 12) tier = 'cluster';
    else if (k >= 12) tier = 'detail';

    if (this.currentTier() !== tier) {
      this.currentTier.set(tier);
    }

    // Update date range pill
    const domain = zx.domain();
    const fmt = d3.timeFormat('%b %d, %Y');
    this.dateRange.set(`${fmt(domain[0])} — ${fmt(domain[1])}`);

    // Update Axes
    const xAxis = d3.axisBottom(zx).ticks(iw / 80);
    const yAxis = d3.axisLeft(this.y).ticks(5).tickFormat(d => `$${d3.format('~s')(d as number)}`);
    
    this.svg.select<SVGGElement>('g.x-axis').call(xAxis as any);
    this.svg.select<SVGGElement>('g.y-axis').call(yAxis as any);
    this.svg.select<SVGGElement>('g.grid').call(
      d3.axisLeft(this.y).ticks(5).tickSize(-iw).tickFormat(() => '') as any
    ).call(sel => sel.select('.domain').remove());

    // Filter data to visible domain
    const visibleData = data.filter(d => {
      const date = new Date(d.date);
      return date >= domain[0] && date <= domain[1];
    });

    // Handle Tiers
    this.renderTier(tier, visibleData, zx, ih, k);
  }

  private renderTier(tier: ZoomTier, data: TransactionModel[], zx: d3.ScaleTime<number, number>, ih: number, k: number) {
    const gOverview = this.svg!.select<SVGGElement>('g.tier-overview');
    const gCluster = this.svg!.select<SVGGElement>('g.tier-cluster');
    const gDetail = this.svg!.select<SVGGElement>('g.tier-detail');

    const duration = 200;

    gOverview.transition().duration(duration).style('opacity', tier === 'overview' ? 1 : 0)
      .attr('pointer-events', tier === 'overview' ? 'all' : 'none');
    gCluster.transition().duration(duration).style('opacity', tier === 'cluster' ? 1 : 0)
      .attr('pointer-events', tier === 'cluster' ? 'all' : 'none');
    gDetail.transition().duration(duration).style('opacity', tier === 'detail' ? 1 : 0)
      .attr('pointer-events', tier === 'detail' ? 'all' : 'none');

    if (tier === 'overview') {
      this.drawAggregated(gOverview, data, zx, 'month');
    } else if (tier === 'cluster') {
      this.drawAggregated(gCluster, data, zx, 'week');
    } else {
      this.drawDetail(gDetail, data, zx, ih);
    }
  }

  private drawAggregated(g: d3.Selection<SVGGElement, unknown, null, undefined>, data: TransactionModel[], zx: d3.ScaleTime<number, number>, interval: 'month' | 'week') {
    const bucket = interval === 'month' ? d3.timeMonth : d3.timeWeek;
    
    const rolled = d3.rollup(data, 
      v => d3.sum(v, d => Math.abs(d.amount)),
      d => d.category,
      d => +bucket.floor(new Date(d.date))
    );

    const points: AggregatedPoint[] = [];
    const maxTotal = d3.max(Array.from(rolled.values()).flatMap(dates => Array.from(dates.values()))) || 1000;
    const rScale = d3.scaleSqrt().domain([0, maxTotal]).range([8, 45]);

    for (const [category, dates] of rolled) {
      for (const [ts, total] of dates) {
        points.push({
          id: `${category}|${ts}`,
          category,
          date: new Date(ts),
          amount: total,
          radius: rScale(total)
        });
      }
    }

    const styles = this.categoryService.styleByName();

    const bubbles = g.selectAll<SVGGElement, AggregatedPoint>('g.bubble')
      .data(points, d => d.id);

    bubbles.exit().remove();

    const enter = bubbles.enter().append('g').attr('class', 'bubble')
      .style('cursor', 'pointer')
      .on('click', (event, d) => {
        const start = d.date;
        const end = interval === 'month' ? d3.timeMonth.offset(start, 1) : d3.timeWeek.offset(start, 1);
        this.zoomToRange(start, end);
      });

    enter.append('circle');
    enter.append('path').attr('class', 'icon');
    enter.append('text').attr('class', 'label');

    const all = enter.merge(bubbles);

    all.attr('transform', d => `translate(${zx(d.date)}, ${this.y(d.amount)})`);

    all.select('circle')
      .attr('r', d => d.radius)
      .attr('fill', d => styles.get(d.category)?.color || '#94a3b8')
      .attr('fill-opacity', 0.65);

    all.select('path.icon')
      .attr('d', d => d.radius > 14 ? (styles.get(d.category)?.iconPath || '') : null)
      .attr('transform', d => `translate(${-10}, ${-10}) scale(0.83)`) // 24x24 -> ~20x20
      .attr('stroke', '#fff')
      .attr('stroke-width', 2)
      .attr('fill', 'none');

    all.select('text.label')
      .attr('y', d => d.radius + 12)
      .attr('text-anchor', 'middle')
      .attr('fill', '#cbd5f5')
      .attr('font-size', '10px')
      .text(d => d.radius > 25 ? `${d.category} $${Math.round(d.amount)}` : '');
  }

  private drawDetail(g: d3.Selection<SVGGElement, unknown, null, undefined>, data: TransactionModel[], zx: d3.ScaleTime<number, number>, ih: number) {
    const sorted = [...data].sort((a, b) => Math.abs(b.amount) - Math.abs(a.amount));
    const limited = sorted.slice(0, 500);
    this.showTop500Hint.set(data.length > 500);

    const rScale = d3.scaleSqrt().domain(this.y.domain()).range([4, 22]);
    const styles = this.categoryService.styleByName();

    const circles = g.selectAll<SVGGElement, TransactionModel>('g.tx-circle')
      .data(limited, d => d.id);

    circles.exit().remove();

    const enter = circles.enter().append('g').attr('class', 'tx-circle')
      .style('cursor', 'pointer')
      .on('click', (event, d) => this.selectedTransaction.set(d));

    enter.append('circle');
    enter.append('path').attr('class', 'icon');
    enter.append('g').attr('class', 'label-group');

    const all = enter.merge(circles);

    all.attr('transform', d => `translate(${zx(new Date(d.date))}, ${this.y(Math.abs(d.amount))})`);

    all.select('circle')
      .attr('r', d => rScale(Math.abs(d.amount)))
      .attr('fill', d => styles.get(d.category)?.color || '#94a3b8')
      .attr('stroke', d => d3.color(styles.get(d.category)?.color || '#94a3b8')?.darker(1).toString() || '#000')
      .attr('stroke-width', 1);

    all.select('path.icon')
      .attr('d', d => {
        const r = rScale(Math.abs(d.amount));
        return r > 15 ? (styles.get(d.category)?.iconPath || '') : null;
      })
      .attr('transform', `translate(-8, -8) scale(0.66)`) // 24x24 -> 16x16
      .attr('stroke', '#fff')
      .attr('stroke-width', 2)
      .attr('fill', 'none');

    // Greedy labeling
    const placedBoxes: DOMRect[] = [];
    const self = this;
    all.select('g.label-group').each(function(d) {
      const group = d3.select(this);
      group.selectAll('*').remove();
      
      const r = rScale(Math.abs(d.amount));
      if (r < 8) return;

      const text = `${d.description.slice(0, 18)}... $${Math.abs(d.amount)}`;
      const label = group.append('text')
        .attr('y', -r - 5)
        .attr('text-anchor', 'middle')
        .attr('fill', '#f1f5f9')
        .attr('font-size', '10px')
        .attr('font-weight', '500')
        .text(text);

      const node = label.node() as SVGTextContentElement;
      if (!node) return;
      const bbox = node.getBBox();
      const absBox = new DOMRect(
        zx(new Date(d.date)) - bbox.width/2, 
        self.y(Math.abs(d.amount)) - r - 5 - bbox.height, 
        bbox.width, 
        bbox.height
      );

      let collide = false;
      for (const box of placedBoxes) {
        if (!(absBox.right < box.left || absBox.left > box.right || absBox.bottom < box.top || absBox.top > box.bottom)) {
          collide = true;
          break;
        }
      }

      if (collide) {
        label.remove();
      } else {
        placedBoxes.push(absBox);
      }
    });
  }

  private zoomToRange(start: Date, end: Date) {
    if (!this.svg || !this.zoom) return;
    const width = this.chartHost()?.nativeElement.clientWidth || 0;
    const margin = { left: 40, right: 20 };
    const iw = width - margin.left - margin.right;

    const fullDomain = this.x0.domain();
    const totalMs = fullDomain[1].getTime() - fullDomain[0].getTime();
    const targetMs = end.getTime() - start.getTime();
    const k = Math.min(100, Math.max(1, totalMs / targetMs));

    const tx = -this.x0(start) * k;
    
    this.svg.transition().duration(500)
      .call(this.zoom.transform, d3.zoomIdentity.scale(k).translate(tx / k, 0));
  }

  editTransaction(tx: TransactionModel) {
    this.editingTransaction.set(tx);
    this.selectedTransaction.set(null);
  }
}
