FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj src/NetworkMonitoring.Probe/
COPY src/NetworkMonitoring.Domain/SeedWork/NetworkMonitoring.Domain.csproj src/NetworkMonitoring.Domain/SeedWork/

RUN dotnet restore src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj

COPY src/ src/

RUN dotnet publish src/NetworkMonitoring.Probe/NetworkMonitoring.Probe.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y --no-install-recommends tshark ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "NetworkMonitoring.Probe.dll"]
