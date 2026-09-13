# ── STAGE 1: Build ────────────────────────────────────────────
# Build context must be the repository root (PaymentGateway/), not the
# NotificationService folder — this service references the shared
# Contracts project, which lives outside NotificationService's own folder.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["services/NotificationService/NotificationService.Api/NotificationService.Api.csproj", "services/NotificationService/NotificationService.Api/"]
COPY ["services/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj", "services/NotificationService/NotificationService.Domain/"]
COPY ["services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "services/NotificationService/NotificationService.Infrastructure/"]
COPY ["shared/PaymentGateway.Contracts/PaymentGateway.Contracts.csproj", "shared/PaymentGateway.Contracts/"]

RUN dotnet restore "services/NotificationService/NotificationService.Api/NotificationService.Api.csproj"

COPY . .

WORKDIR /src/services/NotificationService/NotificationService.Api
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish


# ── STAGE 2: Final runtime image ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]