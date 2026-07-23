# Stack
ASP.NET Core (.NET 10), Dapper (no EF), PostgreSQL on Supabase, custom JWT auth.
Pattern is controller -> service. No MediatR, no repositories, no CQRS.

# Conventions
- Services take DbConnectionFactory db via primary constructor
- Raw SQL in C# raw string literals ("""), Dapper QueryAsync/ExecuteAsync
- Every query filters by user_id from ClaimTypes.NameIdentifier
- DTOs are records in Models/
- Register new services in Program.cs

# Rules
- Keep it minimal. Don't add abstractions for a single caller.
- Never write migrations that run at startup — SQL lives in database/SQL_files/ and
  is run manually via DataGrip.omponents/d3-trend-chart.