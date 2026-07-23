import {
    Component, ElementRef, OnDestroy, computed, effect,
    inject, signal, viewChild, Signal
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {toSignal} from '@angular/core/rxjs-interop';
import * as d3 from 'd3';
import {TransactionModel} from "../../models/transaction.model";
import {TransactionService} from "../transaction-list/transaction.service";

type ChartStyle = 'area' | 'line' | 'bars';
type Grouping = 'week' | 'month';

interface TrendPoint {
    date: Date;
    total: number;
}

const PALETTE = [
    '#93b4f8', '#4ade80', '#fbbf24', '#f87171', '#c084fc',
    '#38bdf8', '#fb923c', '#f472b6', '#a3e635', '#94a3b8',
];

@Component({
    selector: 'app-d3-trend-chart',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './d3-trend-chart.html',
    styleUrl: './d3-trend-chart.scss',
})
export class D3TrendChartComponent implements OnDestroy {
    private transactionService = inject(TransactionService);

    // plain (non-required) viewChild: the effects below re-run automatically
    // once the query resolves, so we just skip the frame where it's undefined
    private chartHost = viewChild<ElementRef<HTMLDivElement>>('chartHost');

    // ---- customisation state ----
    chartStyle = signal<ChartStyle>('area');
    grouping = signal<Grouping>('month');
    selectedCategories = signal<Set<string>>(new Set()); // empty = all

    styles: { key: ChartStyle; label: string }[] = [
        { key: 'area', label: 'Area' },
        { key: 'line', label: 'Line' },
        { key: 'bars', label: 'Bars' },
    ];

    groupings: { key: Grouping; label: string }[] = [
        { key: 'month', label: 'Monthly' },
        { key: 'week', label: 'Weekly' },
    ];

    // ---- data (shared cached stream — zero extra HTTP calls) ----
    private transactions:Signal<TransactionModel[]> = toSignal(
        this.transactionService.getAllTransactions(),
        { initialValue: [] as TransactionModel[] }
    );

    private expenses = computed(() =>
        this.transactions().filter(t => t.amount < 0));

    categories = computed(() => {
        const totals = d3.rollup(
            this.expenses(),
            v => d3.sum(v, t => Math.abs(t.amount)),
            t => t.category);
        return [...totals.entries()]
            .sort((a, b) => b[1] - a[1])
            .map(([name]) => name);
    });

    categoryColor(cat: string): string {
        const idx = this.categories().indexOf(cat);
        return PALETTE[idx % PALETTE.length];
    }

    isSelected(cat: string): boolean {
        const sel = this.selectedCategories();
        return sel.size === 0 || sel.has(cat);
    }

    toggleCategory(cat: string) {
        const next = new Set(this.selectedCategories());
        next.has(cat) ? next.delete(cat) : next.add(cat);
        this.selectedCategories.set(next);
    }

    clearCategories() {
        this.selectedCategories.set(new Set());
    }

    // ---- aggregated series ----
    private series = computed<TrendPoint[]>(() => {
        const sel = this.selectedCategories();
        const rows = this.expenses().filter(
            t => sel.size === 0 || sel.has(t.category));
        if (!rows.length) return [];

        const bucket = this.grouping() === 'month'
            ? (d: Date) => d3.timeMonth.floor(d)
            : (d: Date) => d3.timeMonday.floor(d);

        const grouped = d3.rollup(
            rows,
            v => d3.sum(v, t => Math.abs(t.amount)),
            t => +bucket(new Date(t.date)));

        return [...grouped.entries()]
            .map(([ts, total]) => ({ date: new Date(ts), total }))
            .sort((a, b) => +a.date - +b.date);
    });

    totalShown = computed(() => d3.sum(this.series(), p => p.total));

    private resizeObserver?: ResizeObserver;
    private svg?: d3.Selection<SVGSVGElement, unknown, null, undefined>;

    constructor() {
        // re-render whenever data or any customisation signal changes
        effect(() => {
            this.series();       // track
            this.chartStyle();   // track
            const host = this.chartHost()?.nativeElement;
            if (!host) return;
            requestAnimationFrame(() => this.render(host));
        });

        effect(() => {
            const host = this.chartHost()?.nativeElement;
            if (!host) return;
            this.resizeObserver?.disconnect();
            this.resizeObserver = new ResizeObserver(() => this.render(host));
            this.resizeObserver.observe(host);
        });
    }

    ngOnDestroy() {
        this.resizeObserver?.disconnect();
    }

    // ================= D3 rendering =================
    private render(host: HTMLDivElement) {
        const data = this.series();
        const width = host.clientWidth;
        const height = host.clientHeight;
        if (!width || !height) return;

        const reduceMotion =
            window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        const dur = reduceMotion ? 0 : 600;

        const margin = { top: 14, right: 14, bottom: 26, left: 46 };
        const iw = width - margin.left - margin.right;
        const ih = height - margin.top - margin.bottom;

        // create the svg once, reuse + transition afterwards
        if (!this.svg) {
            this.svg = d3.select(host).append('svg');
            const g = this.svg.append('g').attr('class', 'plot');
            g.append('defs').append('linearGradient')
                .attr('id', 'trend-gradient')
                .attr('x1', 0).attr('y1', 0).attr('x2', 0).attr('y2', 1)
                .call(grad => {
                    grad.append('stop').attr('offset', '0%')
                        .attr('stop-color', '#5470c6').attr('stop-opacity', 0.55);
                    grad.append('stop').attr('offset', '100%')
                        .attr('stop-color', '#5470c6').attr('stop-opacity', 0.02);
                });
            g.append('g').attr('class', 'grid');
            g.append('g').attr('class', 'x-axis');
            g.append('g').attr('class', 'y-axis');
            g.append('g').attr('class', 'marks');
            g.append('path').attr('class', 'trend-line');
            g.append('g').attr('class', 'hover-layer');
        }

        const svg = this.svg
            .attr('width', width)
            .attr('height', height)
            .attr('viewBox', `0 0 ${width} ${height}`);

        const g = svg.select<SVGGElement>('g.plot')
            .attr('transform', `translate(${margin.left},${margin.top})`);

        const marks = g.select<SVGGElement>('g.marks');
        const linePath = g.select<SVGPathElement>('path.trend-line');

        if (!data.length) {
            marks.selectAll('*').remove();
            linePath.attr('d', null);
            g.select('g.x-axis').selectAll('*').remove();
            g.select('g.y-axis').selectAll('*').remove();
            g.select('g.grid').selectAll('*').remove();
            return;
        }

        // ---- scales ----
        const x = d3.scaleTime()
            .domain(d3.extent(data, d => d.date) as [Date, Date])
            .range([0, iw]);

        const y = d3.scaleLinear()
            .domain([0, (d3.max(data, d => d.total) ?? 0) * 1.1])
            .nice()
            .range([ih, 0]);

        // ---- axes + grid ----
        const monthly = this.grouping() === 'month';
        const xAxis = d3.axisBottom<Date>(x)
            .ticks(Math.min(monthly ? 6 : 8, data.length))
            .tickFormat(d3.timeFormat(monthly ? '%b %y' : '%d %b') as any)
            .tickSizeOuter(0);

        const yAxis = d3.axisLeft<number>(y)
            .ticks(5)
            .tickFormat(d => '$' + d3.format('~s')(d as number));

        g.select<SVGGElement>('g.x-axis')
            .attr('transform', `translate(0,${ih})`)
            .transition().duration(dur)
            .call(xAxis as any);

        g.select<SVGGElement>('g.y-axis')
            .transition().duration(dur)
            .call(yAxis as any);

        g.select<SVGGElement>('g.grid')
            .transition().duration(dur)
            .call(d3.axisLeft(y).ticks(5).tickSize(-iw).tickFormat(() => '') as any)
            .selection()
            .call(sel => sel.select('.domain').remove());

        // ---- marks ----
        const style = this.chartStyle();

        const area = d3.area<TrendPoint>()
            .x(d => x(d.date))
            .y0(ih)
            .y1(d => y(d.total))
            .curve(d3.curveMonotoneX);

        const line = d3.line<TrendPoint>()
            .x(d => x(d.date))
            .y(d => y(d.total))
            .curve(d3.curveMonotoneX);

        if (style === 'bars') {
            linePath.transition().duration(dur / 2).style('opacity', 0);
            marks.selectAll('path.trend-area').remove();

            const barW = Math.max(3, Math.min(34, (iw / data.length) * 0.65));
            marks.selectAll<SVGRectElement, TrendPoint>('rect.trend-bar')
                .data(data, (d: TrendPoint) => +d.date)
                .join(
                    enter => enter.append('rect')
                        .attr('class', 'trend-bar')
                        .attr('x', d => x(d.date) - barW / 2)
                        .attr('width', barW)
                        .attr('y', ih)
                        .attr('height', 0)
                        .attr('rx', 3)
                        .attr('fill', 'url(#trend-gradient)')
                        .attr('stroke', '#5470c6')
                        .attr('stroke-width', 1),
                    update => update,
                    exit => exit.transition().duration(dur / 2)
                        .attr('y', ih).attr('height', 0).remove()
                )
                .transition().duration(dur)
                .attr('x', d => x(d.date) - barW / 2)
                .attr('width', barW)
                .attr('y', d => y(d.total))
                .attr('height', d => ih - y(d.total));
        } else {
            marks.selectAll('rect.trend-bar')
                .transition().duration(dur / 2)
                .attr('y', ih).attr('height', 0).remove();

            // area fill (only in area mode)
            const areaSel = marks
                .selectAll<SVGPathElement, TrendPoint[]>('path.trend-area')
                .data(style === 'area' ? [data] : []);

            areaSel.join(
                enter => enter.append('path')
                    .attr('class', 'trend-area')
                    .attr('fill', 'url(#trend-gradient)')
                    .attr('d', area)
                    .style('opacity', 0)
                    .call(s => s.transition().duration(dur).style('opacity', 1)),
                update => update.transition().duration(dur)
                    .attr('d', area).style('opacity', 1).selection(),
                exit => exit.transition().duration(dur / 2)
                    .style('opacity', 0).remove()
            );

            linePath
                .attr('fill', 'none')
                .attr('stroke', '#93b4f8')
                .attr('stroke-width', 2.5)
                .attr('stroke-linecap', 'round')
                .style('opacity', 1)
                .transition().duration(dur)
                .attr('d', line(data));
        }

        this.attachHover(g, data, x, y, iw, ih);
    }

    // crosshair + tooltip
    private attachHover(
        g: d3.Selection<SVGGElement, unknown, null, undefined>,
        data: TrendPoint[],
        x: d3.ScaleTime<number, number>,
        y: d3.ScaleLinear<number, number>,
        iw: number, ih: number,
    ) {
        const layer = g.select<SVGGElement>('g.hover-layer');
        layer.selectAll('*').remove();

        const cursor = layer.append('g').style('display', 'none');
        cursor.append('line')
            .attr('y1', 0).attr('y2', ih)
            .attr('stroke', 'rgba(148,163,184,0.45)')
            .attr('stroke-dasharray', '3 3');
        cursor.append('circle')
            .attr('r', 4.5)
            .attr('fill', '#93b4f8')
            .attr('stroke', '#0f172a')
            .attr('stroke-width', 2);

        const tip = cursor.append('g');
        const tipBg = tip.append('rect')
            .attr('rx', 6)
            .attr('fill', 'rgba(15,23,42,0.92)')
            .attr('stroke', 'rgba(84,112,198,0.5)');
        const tipDate = tip.append('text')
            .attr('class', 'tip-date').attr('x', 8).attr('y', 15);
        const tipValue = tip.append('text')
            .attr('class', 'tip-value').attr('x', 8).attr('y', 32);

        const bisect = d3.bisector<TrendPoint, Date>(d => d.date).center;
        const fmt = d3.timeFormat(this.grouping() === 'month' ? '%B %Y' : 'Week of %d %b');

        layer.append('rect')
            .attr('width', iw).attr('height', ih)
            .attr('fill', 'transparent')
            .on('pointerenter', () => cursor.style('display', null))
            .on('pointerleave', () => cursor.style('display', 'none'))
            .on('pointermove', (event: PointerEvent) => {
                const [mx] = d3.pointer(event);
                const d = data[bisect(data, x.invert(mx))];
                if (!d) return;

                const px = x(d.date);
                cursor.select('line').attr('x1', px).attr('x2', px);
                cursor.select('circle').attr('cx', px).attr('cy', y(d.total));

                tipDate.text(fmt(d.date));
                tipValue.text('$' + d3.format(',.2f')(d.total));
                const w = Math.max(
                    (tipDate.node()?.getComputedTextLength() ?? 0),
                    (tipValue.node()?.getComputedTextLength() ?? 0)) + 16;
                tipBg.attr('width', w).attr('height', 40);
                const tx = px + w + 12 > iw ? px - w - 10 : px + 10;
                tip.attr('transform', `translate(${tx},${Math.max(0, y(d.total) - 48)})`);
            });
    }
}
