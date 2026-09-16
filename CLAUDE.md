# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is
Personal expense tracker: Angular 21 frontend (`frontend_basic_setup/`, also shipped as a Capacitor Android app) + ASP.NET Core 10 API (`backend_api/expense_tracker/`) + PostgreSQL (`database/SQL_files/`). Deployed via GitHub Actions to Azure Static Web Apps (frontend) and Azure Web App (API) on push to `main`.

Two `.junie/guidelines.md` files (one per half) hold the owner's conventions; they are summarised below and should be kept in sync if either changes.

## Commands

### Backend (run from `backend_api/expense_tracker/`)
- `dotnet run` — starts API on https://localhost:7283 / http://localhost:5292 (Development env; Swagger at `/swagger`)
- `dotnet build` / `dotnet restore`
- Needs a local Postgres per `appsettings.Development.json` (db `expense_tracker`) and JWT keys in `Utils/Keys/` (gitignored; production uses `JWT_PUBLIC_KEY` env var).
- There is no test project. CI runs `dotnet test` on the csproj but it is a no-op.

### Frontend (run from `frontend_basic_setup/`)
- `npm start` — `ng serve` on :4200 with `proxy.conf.json` forwarding `/api` → https://localhost:7283 (so `environment.dev.ts` has `apiUrl: ''`)
- `npm run build` — outputs to `dist/app-v0.1/browser` (this path is what Capacitor and the Azure SWA workflow both consume)
- `npm run build -- --configuration production` — what CI runs; bundle budgets in `angular.json` fail the build if exceeded
- `npx cap sync android` / `npx cap open android` — Capacitor Android (webDir is the build output above)
- No unit tests or lint are configured.

### Database
- Schema changes are plain SQL in `database/SQL_files/migrations/NN_*.sql`, numbered sequentially (latest is `10_`). They are run **manually** (DataGrip) — never add startup migrations or EF. Write them idempotent (`IF NOT EXISTS`, "safe to re-run").

## Architecture

### Backend
- Pattern is strictly **controller → service → Dapper raw SQL**. No EF, no repositories, no MediatR/CQRS. Services take `DbConnectionFactory` via primary constructor and write SQL in C# raw string literals (`"""`), using `conn.QueryAsync/ExecuteAsync`. See `Services/BudgetService.cs` as the canonical example.
- Every service is registered explicitly in `Program.cs` (`AddScoped<...>`); add new ones there.
- Auth: custom RSA-signed JWT (`AuthService`/`TokenService`). Controllers are `[Authorize]` and get the user via `User.FindFirstValue(ClaimTypes.NameIdentifier)`; **every query must filter by `user_id`**.
- DTOs are `record`s in `Models/` (`dtos.cs`, `CategoryDtos.cs`, etc.).
- Category classification (`CategoryClassifierService`): priority is transaction_type shortcuts → user `merchant_rules` → global rules (cached 10 min) → "Other". Categories can be global (`user_id IS NULL`) or per-user; transactions store the category **by name**, not id.
- CSV import (`CsvImportService`, `POST api/import/transactions`) classifies on the way in and dedups (migration `10_import_dedup.sql`).
- Errors bubble to a global handler in `Program.cs` that returns `{message, stack}` as 500.

### Frontend
- Angular 21 **standalone components + signals only**. No NgModules, no state library. `inject()` over constructors. Services expose Observables; components convert with `toSignal`. RxJS in components only via `toSignal`.
- Template control flow: `@if` / `@for` only (never `*ngIf`/`*ngFor`). Forms: signal forms.
- Layout: `app.routes.ts` → `LayoutComponent` (guarded by `AuthGuard`) with lazy-loaded pages under `pages/` (home, transactions, planning). Reusable widgets live in `components/`, each in its own directory (feature-split, not type-split).
- API calls: `environment.apiUrl + '/api/...'`; `authInterceptor` (components/auth) attaches the JWT from localStorage. `RefreshService.refresh$` (data/) is the manual cross-component refresh trigger.
- Charts: ngx-echarts (ECharts core provided in `app.config.ts`) and raw d3. Chart hosts need a concrete pixel height (`flex:1` + `min-height:Xpx` on the element itself) and `min-height:0` on flex ancestors.
- Styling: SCSS per component, dark theme, mobile-first. Some Tailwind utilities exist but prefer SCSS. Match the visual style of `components/d3-trend-chart`. UI wording: "tabs", not "pills".

## Rules from the owner
- Keep it minimal. Don't add abstractions for a single caller. No new libraries without asking.
- Don't refactor files you weren't asked to touch.
