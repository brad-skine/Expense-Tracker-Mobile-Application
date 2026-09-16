Use modern file layout with components having their own directory. Split by features not by type.
Maintainable code.
Clean code.
Angular 21 use best modern practices
If making form use signal forms.

# Conventions
- inject() over constructor injection
- signal / computed / toSignal; services expose Observables, components convert
- new control flow only: @if / @for (never *ngIf / *ngFor)
- API base: environment.apiUrl + '/api/...'
- D3/ECharts hosts need a concrete pixel height (flex:1 + min-height:Xpx on the
  element itself, not a wrapper), and min-height:0 on flex ancestors
- UI wording: "tabs", not "pills"

# Rules
- Keep it minimal. No new libraries without asking. No state management library.
