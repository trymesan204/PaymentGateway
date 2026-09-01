# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository structure

This is a multi-service .NET solution under `services/`, each service independently buildable/runnable (own `.sln`, no shared solution at the repo root):

- `services/PaymentService/` — the mature service. Accepts payment requests, calls a (currently mocked) payment processor, persists results to Postgres.
- `services/LedgerService/` — a brand-new scaffold (untracked in git as of this writing). `Program.cs` is still the default ASP.NET template, `ILedgerRepository` and `LedgerDbContext` are empty stubs. Treat it as not-yet-implemented rather than as a reference for existing patterns — mirror `PaymentService`'s structure/conventions instead when building it out.

Each service follows Clean Architecture-ish layering:
- `<Service>` / `<Service>.Api` — ASP.NET Core Web API host: controllers, DI wiring, `Program.cs`, `appsettings*.json`.
- `<Service>.Domain` — entities, enums, and interfaces (`IPaymentRepository`, `IPaymentProcessor`). No dependencies on other layers.
- `<Service>.Infrastructure` — EF Core `DbContext`, entity configurations, migrations, repository implementations, and an `AddInfrastructure(...)` extension method that wires DbContext/repositories/processors into DI. This is the only project that knows about Postgres/Npgsql.

## Common commands

Run all commands from inside the relevant service directory (e.g. `services/PaymentService/`), since there's no root-level solution.

```bash
# Build
dotnet build PaymentService.sln

# Run the API (from services/PaymentService/PaymentService/)
dotnet run --project PaymentService

# EF Core migrations (from services/PaymentService/, targeting the Infrastructure project)
dotnet ef migrations add <Name> --project PaymentService.Infrastructure --startup-project PaymentService
dotnet ef database update --project PaymentService.Infrastructure --startup-project PaymentService

# Run via Docker Compose (Postgres + API), from services/PaymentService/
docker compose up --build
```

Notes:
- `PaymentService.Infrastructure/PaymentDbContextFactory.cs` supplies a design-time `DbContext` (hardcoded local connection string) so `dotnet ef` commands work without running the full host.
- `PaymentService.Tests` currently has no test framework references in its `.csproj` and no test source files — there is no working test suite to run yet. If asked to add tests, you'll need to add a test framework (xUnit/NUnit/etc.) and its packages first.
- The Dockerfile and `docker-compose.yml` build context assumes `docker compose` is invoked from `services/PaymentService/` (paths inside `docker-compose.yml` are relative to that directory, e.g. `dockerfile: ./PaymentService/Dockerfile`).

## Architecture notes (PaymentService)

- **Idempotency**: `CreatePaymentRequest.IdempotencyKey` (client-supplied GUID) is checked via `IPaymentRepository.GetByIdempotencyKeyAsync` before creating a new `Payment`. If a payment already exists for that key, the existing one is returned instead of creating a duplicate (`PaymentResult.IsNew` flag distinguishes the two cases for the controller to pick `200 OK` vs `201 Created`).
- **Payment processing flow** (`PaymentsService.ProcessPaymentAsync`): create `Payment` row → call `IPaymentProcessor.ProcessPaymentAsync` → update row with resulting status. A `TimeoutException` from the processor is caught and mapped to `PaymentStatus.Pending` (not `Failed`) — the payment is left to be reconciled/retried later rather than treated as a hard failure.
- **`IPaymentProcessor`** is currently backed by `MockPaymentProcessor`, which randomly returns `Succeeded`/`Failed`/throws `TimeoutException` to simulate a real payment provider. Swapping in a real processor means implementing `IPaymentProcessor` and changing the registration in `Infrastructure/DependencyInjection.cs`.
- **`PaymentStatus` enum is stored as a string** in Postgres (`HasConversion<string>()` in `PaymentDbContext.OnModelCreating`), and serialized as a string over the API (`JsonStringEnumConverter` in `Program.cs`) — don't reorder/rely on enum ordinal values.
- Logging is via Serilog, configured from `appsettings.json` (console + rolling daily file sink under `logs/`), with request logging enabled via `UseSerilogRequestLogging()`.
- Swagger/OpenAPI UI is currently enabled unconditionally (not gated behind `IsDevelopment()` — that check is commented out in `Program.cs`).
