FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj src/NetworkMonitoring.Domain/SeedWork/
COPY src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj src/NetworkMonitoring.IntegrationConsole/
RUN dotnet restore src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj
COPY src/NetworkMonitoring.Domain/ src/NetworkMonitoring.Domain/
COPY src/NetworkMonitoring.IntegrationConsole/ src/NetworkMonitoring.IntegrationConsole/
RUN dotnet publish src/NetworkMonitoring.IntegrationConsole/NetworkMonitoring.IntegrationConsole.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetworkMonitoring.IntegrationConsole.dll"]
