FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj src/NetworkMonitoring.Domain/SeedWork/
COPY src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj src/NetworkMonitoring.Backend/
RUN dotnet restore src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj

COPY src/NetworkMonitoring.Domain/ src/NetworkMonitoring.Domain/
COPY src/NetworkMonitoring.Backend/ src/NetworkMonitoring.Backend/
RUN dotnet publish src/NetworkMonitoring.Backend/NetworkMonitoring.Backend.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetworkMonitoring.Backend.dll"]
