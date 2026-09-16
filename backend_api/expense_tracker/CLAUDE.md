- DI registrations live in Program.cs (scoped services, singleton DbConnectionFactory).
- Get user with `Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`.
- SQL as raw string literals in the service; column aliases map to PascalCase properties.
- Dedup: CSV rows use `ON CONFLICT (user_id, transaction_date, amount, balance)`;
  Akahu rows use `ON CONFLICT (user_id, external_id) WHERE external_id IS NOT NULL`.
- Verify with Swagger at /swagger (JWT bearer). Run: `dotnet watch run`.
