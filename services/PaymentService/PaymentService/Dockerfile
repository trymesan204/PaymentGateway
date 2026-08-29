#── STAGE 1: "Build Kitchen" ─────────────────────────────────────
# This is a temporary workspace, like a kitchen where you cook a meal.
# It has all the tools (the full .NET SDK) needed to compile your code.
# We will throw this whole kitchen away at the end — we only keep the finished meal.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
 
# Set the "current folder" inside this temporary box to /src
WORKDIR /src
 
# Copy ONLY the .csproj files first (not all your code yet).
# Why? Docker is smart — if these files don't change, it reuses cached work
# instead of re-downloading packages every single time you rebuild. Saves time.
COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
COPY ["PaymentService.Domain/PaymentService.Domain.csproj", "PaymentService.Domain/"]
COPY ["PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService.Infrastructure/"]
 
# Now download all the NuGet packages each project needs (EF Core, etc.)
RUN dotnet restore "PaymentService/PaymentService.csproj"
 
# NOW copy the rest of your actual code (controllers, entities, everything else)
COPY . .
 
# Move into the API project folder and compile it into a "Release" build
WORKDIR /src
RUN dotnet build "PaymentService/PaymentService.csproj" -c Release -o /app/build
 
# Package the compiled app into its final, ready-to-run form
RUN dotnet publish "PaymentService/PaymentService.csproj" -c Release -o /app/publish
 
 
# ── STAGE 2: "The Actual Sealed Box" ─────────────────────────────
# This is a much smaller, leaner box — it only has what's needed to RUN
# the app, not build it. No SDK, no source code, no clutter.
# This is the box that actually gets shipped/deployed.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
 
WORKDIR /app
 
# Copy ONLY the finished, compiled app from Stage 1's "publish" folder.
# Nothing else from the build kitchen comes along — keeps the final box small.
COPY --from=build /app/publish .
 
# Tell Docker "this app listens on port 8080 inside the box"
EXPOSE 8080
 
# The command that runs when someone opens this box and the app starts
ENTRYPOINT ["dotnet", "PaymentService.dll"]
 