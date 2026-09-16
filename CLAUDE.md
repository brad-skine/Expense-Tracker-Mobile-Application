# Expense Tracker (personal finance app "Junie")

Monorepo: `backend_api/expense_tracker` (.NET 10, Dapper, PostgreSQL on Supabase),
`frontend_basic_setup` (Angular 21 standalone + signals, Capacitor Android, D3/ECharts),
`database/SQL_files/migrations` (manual numbered SQL, run in DataGrip — never by the app).

## Rules
- Minimal solutions. No new libraries, abstractions or patterns without asking.
- Controller → Service → Dapper SQL. No repositories, MediatR, EF, AutoMapper.
- Every SQL touching `transactions` filters by `user_id`.
- Never load-per-row inside loops (classifier rules, lookups). Load once, apply in memory.
- Secrets only via env vars / user-secrets / App Service settings. Never in appsettings*.json, never in git.
- Match existing file naming, folder layout, SCSS tokens and service patterns exactly. Copy from a neighbour file.
- Before claiming done: `dotnet build` (backend) and `npx tsc --noEmit` (frontend) must pass. Show output.
- Migrations: next number = highest in `migrations/` + 1. Idempotent. Commented header like `10_import_dedup.sql`.
- Don't commit or push. Draft the commit message; the human commits.

## Bank integration
- Akahu Personal App, static tokens (`AKAHU_APP_TOKEN`, `AKAHU_USER_TOKEN`).
- `bank_connections` table; `transactions.external_id` = Akahu `_id` for dedup.
- Account filtering: `accountIds` query param, empty = all.
- Sample Akahu responses in `backend_api/expense_tracker/docs/akahu-samples/`.
