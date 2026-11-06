# Development stage, selected by devcontainer.json
FROM mcr.microsoft.com/devcontainers/dotnet:10.0 AS dev
WORKDIR /app

# Production publish stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore CloneDevOpsTemplate/CloneDevOpsTemplate.csproj
RUN dotnet publish CloneDevOpsTemplate/CloneDevOpsTemplate.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS production
WORKDIR /app
EXPOSE 8080
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloneDevOpsTemplate.dll"]
