# --- Stage 1: Development Environment ---
FROM mcr.microsoft.com/devcontainers/dotnet:10.0 AS dev
WORKDIR /app

# --- Stage 2: Build Stage for Production ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the source code
COPY . .
RUN dotnet restore
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Stage 3: Lean Production Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
EXPOSE 8080
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloneDevOpsTemplate.dll"]