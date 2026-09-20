# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

PaymentGateway is a microservices payment platform demonstrating event-driven architecture, double-entry bookkeeping, and reliability patterns (idempotency, transactional outbox, at-least-once consumers). It is not a real payment processor — `MockPaymentProcessor` simulates an external provider. See `README.md` for the full architecture diagram, demo flow, and a "Known limitations" table (no auth/gateway yet, no DLQ, balance check has a check-then-act race, etc.) — read it before assuming something is a bug rather than a documented scope cut.

## Repository structure

Three independently buildable/runnable .NET 8 services under `services/`, each with its own `.sln`, plus a shared contracts library and a root `docker-compose.yml`/`PaymentGateway.sln` that wire everything together for local dev:

- `services/PaymentService/` — accepts payment requests, calls the mocked payment processor, persists results to Postgres, publishes `PaymentSucceededEvent` via a transactional outbox.
- `services/LedgerService/` — consumes `PaymentSucceededEvent`, maintains double-entry balances (`ledger_entries`) and exposes read endpoints; also has a dev-only balance-seeding endpoint.
- `services/NotificationService/` — consumes `PaymentSucceededEvent`, sends emails via Mailtrap, logs delivery to `notification_logs`.
- `shared/PaymentGateway.Contracts/` — the only project referenced by more than one service: `PaymentSucceededEvent` (record) and `PaymentType` (enum), serialized to/from JSON as the wire format published on RabbitMQ. Changing this record's shape affects three services at once — check `NotificationService.Infrastructure/Consumers` and `LedgerService`'s consumer alongside `PaymentService.Infrastructure/Messaging/RabbitMqEventPublisher.cs`.

Each service follows the same Clean Architecture-ish layering (PaymentService is the most mature; mirror its conventions when extending the others):
- `<Service>` / `<Service>.Api` — ASP.NET Core Web API host: controllers, DI wiring, `Program.cs`, `appsettings*.json`, `Dockerfile`.
- `<Service>.Domain` — entities, enums, interfaces (e.g. `IPaymentRepository`, `IPaymentProcessor`, `ILedgerRepository`). No dependencies on other layers.
- `<Service>.Infrastructure` — EF Core `DbContext`, entity configurations, migrations, repository implementations, RabbitMQ publisher/consumer, and an `AddInfrastructure(...)` extension method that wires everything into DI. This is the only project per service that knows about Postgres/Npgsql. Each has a `<Service>DbContextFactory.cs` supplying a design-time `DbContext` (hardcoded local connection string) so `dotnet ef` works without running the full host.

## Common commands

Run commands from inside the relevant service directory (e.g. `services/PaymentService/`) — there's no build/test that spans all three services other than opening the root `PaymentGateway.sln` in an IDE.

```bash
# Build a single service
dotnet build PaymentService.sln     # or LedgerService.sln / NotificationService.sln

# Run the API (from the service's Api project directory, e.g. services/PaymentService/PaymentService/)
dotnet run --project PaymentService     # project name varies: PaymentService, LedgerService.Api, NotificationService.Api

# EF Core migrations (from the service root, targeting its Infrastructure project)
dotnet ef migrations add <Name> --project PaymentService.Infrastructure --startup-project PaymentService
dotnet ef database update --project PaymentService.Infrastructure --startup-project PaymentService

# Run everything together (Postgres x3 + RabbitMQ + all three services), from the repo root
docker compose up --build
```

Notes:
- No service has a real test suite yet: every `*.Tests.csproj` (only `PaymentService.Tests` currently exists) has no test framework package references and no test source files, despite the README mentioning xUnit — treat "add tests" requests as needing the framework added from scratch.
- `services/PaymentService/docker-compose.yml` is a standalone (Postgres + RabbitMQ + payment-service only) compose file predating the root one; it does not include Ledger or Notification. Prefer the root `docker-compose.yml` for anything that needs cross-service communication.
- Swagger UI: enabled unconditionally in PaymentService (the `IsDevelopment()` check is commented out in `Program.cs`); properly gated behind `IsDevelopment()` in LedgerService and NotificationService.
- Local ports (root docker-compose): PaymentService 5000, LedgerService 5001, NotificationService 5002, RabbitMQ management UI 15672 (guest/guest), Postgres per service on 5433/5434/5435.

## Cross-service architecture

**Event flow**: PaymentService publishes to a `payment.events` fanout exchange; LedgerService and NotificationService each bind their own durable queue (`ledger.payment-events`, `notification.payment-events`) independently. PaymentService has no knowledge of its consumers — adding a new one requires no upstream change.

**Transactional outbox** (PaymentService only): `PaymentsService.ProcessPaymentAsync` never publishes to RabbitMQ directly. It writes the payment update and an `OutboxMessage` row in one DB transaction (`IUnitOfWork.SaveChangesAsync`); `OutboxRelayService` (a `BackgroundService`) polls unpublished rows every 2s and publishes them via `IEventPublisher`, marking them published only after a successful publish. This avoids the dual-write problem between Postgres and RabbitMQ.

**Idempotent consumers**: both LedgerService and NotificationService check a `processed_events` table (keyed on the event's `EventId`) before doing any work, since RabbitMQ fanout delivery is at-least-once, not exactly-once. Each consumer `BasicNack`s with `requeue: true` on failure (there is retry-count/DLQ-branching logic in the consumer, but no dead-letter exchange is actually declared — see README's "Known limitations").

**Double-entry bookkeeping** (LedgerService): every `PaymentSucceededEvent` produces exactly one debit and one credit `LedgerEntry`, each with a running `BalanceAfter` snapshot — balance is derived from the latest entry, never stored as a single mutable field. `TopUp` debits a fixed system suspense account (`LedgerAccounts.SystemSuspenseAccountId`, `LedgerService.Domain/Constants`); `Transfer`/`MerchantPayment` debits the payer's own account. `LedgerEventHandler.HandlePaymentSucceededAsync` and `ILedgerRepository.AddEntriesAndMarkProcessedAsync` are the two places that matter if you touch this.

**Synchronous cross-service call**: PaymentService calls LedgerService's `GET /accounts/{accountId}/balance` via `ILedgerClient` (`PaymentService.Infrastructure/Services/LedgerClient.cs`, HTTP client registered through `AddHttpClient<ILedgerClient, LedgerClient>` in `DependencyInjection.cs`, base URL from config key `Ledger:BaseUrl`) before allowing a `Transfer`/`MerchantPayment`, to reject insufficient-funds requests before ever creating the `Payment` row. This is the one place PaymentService depends synchronously on another service being up.

**Dev-only balance seeding** (LedgerService): `POST /dev/accounts/{accountId}/seed-balance` (`DevController`) writes a real balanced debit/credit pair directly via `IDevSeedService`/`ILedgerRepository`, skipping RabbitMQ entirely, so a test account can be funded without a real TopUp payment. It's always registered/mapped, but the controller returns `404` unless `IWebHostEnvironment.IsDevelopment()` — don't remove that check or register a similar dev endpoint without one.

## Service-specific notes

**PaymentService**
- Idempotency: `CreatePaymentRequest.IdempotencyKey` is checked via `IPaymentRepository.GetByIdempotencyKeyAsync` before creating a new `Payment`; an existing match is returned instead of duplicating (`PaymentResult.IsNew` drives the controller's `200` vs `201`).
- A `TimeoutException` from `IPaymentProcessor` is mapped to `PaymentStatus.Pending` (not `Failed`) — left for reconciliation, not treated as a hard failure. There is no reconciliation job yet (see README).
- `IPaymentProcessor` is `MockPaymentProcessor`, randomly returning `Succeeded`/`Failed`/throwing `TimeoutException`. Swap it by implementing `IPaymentProcessor` and changing the registration in `Infrastructure/DependencyInjection.cs`.
- `PaymentStatus` is stored as a string in Postgres (`HasConversion<string>()`) and serialized as a string over the API (`JsonStringEnumConverter`) — don't rely on enum ordinal values.

**LedgerService / NotificationService**
- Both consumers (`PaymentSucceededConsumer`) connect to RabbitMQ directly in their constructor (not lazily), declaring the exchange/queue/binding at startup.
- NotificationService currently sends every email to a single hardcoded address (`trymesan204@gmail.com`) in `NotificationEventHandler.BuildAndSendAsync` rather than resolving a real address per account — there's no User/Identity service in scope yet.

## Logging

All three services use Serilog configured from `appsettings.json` (console + rolling daily file sink under `logs/`), with `UseSerilogRequestLogging()` enabled.
