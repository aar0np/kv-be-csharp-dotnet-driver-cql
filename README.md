# KillrVideo v2 - Dotnet C-Sharp Boot Backend

Date: November 2025

A reference backend for the KillrVideo sample application rebuilt for 2025 using **Dotnet**, **C-Sharp** and **DataStax Astra DB**.

---

## Overview
This repo demonstrates modern API best-practices with:

* Restful, typed request/response models
* Role-based JWT auth
* DataStax's Object Mapper from the C Sharp driver API client via `CassandraCSharpDriver -v 3.22.0`
* Micro-service friendly layout – or run everything as a monolith

---

## Prerequisites
1. **Dotnet 9+** runtime
2. A **DataStax Astra DB** serverless database – [grab a free account](https://astra.datastax.com).

## Setup & Configuration
```bash
# clone
git clone git@github.com:KillrVideo/kv-be-csharp-dotnet-driver-cql.git
cd kv-be-csharp-dotnet-driver-cql

# build and install deps
dotnet build
```

Database schema:
1. Create a new keyspace named `killrvideo`.
2. Create the tables from the CQL file in the killrvideo-data repository: <https://github.com/KillrVideo/killrvideo-data/blob/master/schema-astra.cql>

Environment variables (via `export`):

| Variable | Description |
|----------|-------------|
| `ASTRA_DB_SECURE_BUNDLE_LOCATION` | Downloaded from the Astra UI once you have created your database |
| `ASTRA_DB_APPLICATION_TOKEN` | Token created in Astra UI |
| `ASTRA_DB_KEYSPACE` | `killrvideo` |

Edit `appsettings.json`:
 - Generate and change the `jwt.key` key (or use the default).

Command line
 - Trust the ASP.NET Core HTTPS dev certificate. [documentation](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-9.0&tabs=visual-studio%2Clinux-sles#trust-the-aspnet-core-https-development-certificate)
```bash
dotnet dev-certs https trust
```
_Note: If you have trouble with the certificate in your browser or via `curl`, try "cleaning" the Dotnet certificate store, and "trust" again. `dotnet dev-certs https clean`_

---

## Running the Application
```bash
dotnet build
dotnet run
```
Or simply...
```bash
dotnet run
```

## Test the health check service
```bash
curl -X GET "https://localhost:7264/api/v1/health" \
--header "Content-Type: application/json" \
--http1.0
```
"Service is up and running!"
