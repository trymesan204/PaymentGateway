# ── STAGE 1: Build ────────────────────────────────────────────
# All COPY paths below are relative to the BUILD CONTEXT, which docker-compose.yml
# sets to the repository root (PaymentGateway/) — same as Payment/Ledger/Notification
# Service, so this can't assume the context is the ApiGateway folder itself.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["services/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj", "services/ApiGateway/ApiGateway.Api/"]
RUN dotnet restore "services/ApiGateway/ApiGateway.Api/ApiGateway.Api.csproj"

COPY . .

WORKDIR /src/services/ApiGateway/ApiGateway.Api
RUN dotnet build "ApiGateway.Api.csproj" -c Release -o /app/build
RUN dotnet publish "ApiGateway.Api.csproj" -c Release -o /app/publish


# ── STAGE 2: Final runtime image ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ApiGateway.Api.dll"]