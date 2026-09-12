# ── STAGE 1: Build ────────────────────────────────────────────
# All COPY paths below are relative to the BUILD CONTEXT, which must be
# the repository root (PaymentGateway/), not the LedgerService folder —
# this is required because this service references the shared Contracts
# project, which lives outside LedgerService's own folder.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["services/LedgerService/LedgerService.Api/LedgerService.Api.csproj", "services/LedgerService/LedgerService.Api/"]
COPY ["services/LedgerService/LedgerService.Domain/LedgerService.Domain.csproj", "services/LedgerService/LedgerService.Domain/"]
COPY ["services/LedgerService/LedgerService.Infrastructure/LedgerService.Infrastructure.csproj", "services/LedgerService/LedgerService.Infrastructure/"]
COPY ["shared/PaymentGateway.Contracts/PaymentGateway.Contracts.csproj", "shared/PaymentGateway.Contracts/"]

RUN dotnet restore "services/LedgerService/LedgerService.Api/LedgerService.Api.csproj"

COPY . .

WORKDIR /src/services/LedgerService/LedgerService.Api
RUN dotnet build "LedgerService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "LedgerService.Api.csproj" -c Release -o /app/publish


# ── STAGE 2: Final runtime image ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "LedgerService.Api.dll"]