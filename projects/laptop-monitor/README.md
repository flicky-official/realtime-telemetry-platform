# Laptop Monitor

Laptop hardware monitoring using ASP.NET Core, PostgreSQL, and MQTT 5.
A Flutter dashboard and laptop monitoring agent are planned.

## Current status

- PostgreSQL schema and EF Core migrations implemented.
- Database health endpoint tested in the original development environment.
- Latest-reading endpoint tested.
- MQTT background subscriber implemented; currently logs messages only.
- MQTT-to-database persistence is not implemented yet.
- Imported API builds successfully.

## API endpoints

- GET / — API status.
- GET /health/db — executes a PostgreSQL connection test.
- GET /devices/{deviceId}/latest — returns one latest sample, or null
  when the device has no samples.
- Unknown devices return HTTP 404.

## Build

Requires the .NET 10 SDK.

From this directory:

    dotnet restore src/MyApp.Api/MyApp.Api.csproj
    dotnet build src/MyApp.Api/MyApp.Api.csproj --no-restore

## Local configuration

.env.example documents the required settings; it contains no real credentials.

The API reads DATABASE_URL, MQTT_USER, and MQTT_PASSWORD from its process
environment. It does not automatically load .env.

The current API connects to MQTT at 127.0.0.1:1883.
The development API is run directly on the host.

Do not commit .env or mosquitto/config/passwd.

## Migration safety

This folder was imported from SQL-MQTT-FLUTTER.
See IMPORT.md for the source commit.

The original development environment remains the active runtime.
Do not start this folder's Compose stack alongside the original:
both publish the same ports. Moving directories also changes Compose's
default project name and can select different database volumes.

A fresh checkout requires database/user initialization and creation of
the MQTT password file before it can run.

## Relationship to the vehicle demo

The repository-root Compose stack is the existing vehicle HUD demo.
This project's Compose stack is separate; their brokers, authentication,
and message contracts have not yet been unified.
