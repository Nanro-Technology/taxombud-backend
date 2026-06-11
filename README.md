# Tax Ombud Case Management System — Backend API

A production-ready **ASP.NET Core 9** REST API for the **South African Tax Ombud (OTO)** Case Management System.

## Overview

The system enables taxpayers to lodge complaints against SARS (South African Revenue Service), tracks investigations through their full lifecycle, manages internal HR operations, and produces statutory reports.

Built with **Clean Architecture**, **CQRS via MediatR**, and **Entity Framework Core**.

---

## Technology Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 9 (Minimal Hosting) |
| CQRS | MediatR 12 |
| ORM | Entity Framework Core 9 + SQL Server |
| Auth | JWT Bearer + TOTP MFA (Otp.NET) |
| Background Jobs | Hangfire + SQL Server storage |
| Validation | FluentValidation |
| Docs | Swashbuckle (Swagger/OpenAPI) |
| Logging | Serilog (Console + File sinks) |
| Storage | Pluggable `IFileStorageService` |

---

## Architecture

```
TaxOmbud.Domain          ← Entities, value objects, domain events, enums
TaxOmbud.Application     ← CQRS handlers, validators, interfaces
TaxOmbud.Infrastructure  ← EF Core, JWT, Hangfire, file storage, email
TaxOmbud.API             ← ASP.NET Core controllers, middleware, DI
TaxOmbud.KeyGenerator    ← Utility to generate JWT signing keys
```

---

## Endpoints Summary (155 total)

| Controller | Count |
|---|---|
| Auth | 11 |
| Users | 10 |
| Taxpayers | 8 |
| Officers | 6 |
| Departments | 4 |
| Complaints | 18 |
| Cases | 17 |
| Documents | 6 |
| Communications | 3 |
| Appeals | 6 |
| Appointments | 6 |
| Notifications | 5 |
| Reports | 10 |
| Roles | 5 |
| Audit Logs | 2 |
| System | 7 |
| Webhooks | 6 |
| HR | 12 |
| Pay Grades | 7 |
| Health | 4 |
| Search | 1 |
| Lookups | 1 |
| **TOTAL** | **155** |

Full OpenAPI documentation available at `/swagger` when running in Development.

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (local or Docker)

### Configuration

Copy `appsettings.Development.json` and configure:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TaxOmbudDb_Dev;Trusted_Connection=True;"
  },
  "Jwt": {
    "SecretKey": "<generate with TaxOmbud.KeyGenerator>",
    "Issuer": "TaxOmbud.API",
    "Audience": "TaxOmbud.Client"
  }
}
```

### Run

```bash
dotnet restore
dotnet run --project src/TaxOmbud.API
```

The API will:
1. Apply any pending EF Core migrations automatically
2. Seed roles, permissions, lookup data, and a default admin user
3. Start Hangfire background workers
4. Serve Swagger UI at http://localhost:5013

---

## Generating a JWT Key

```bash
dotnet run --project src/TaxOmbud.KeyGenerator
```

---

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/TaxOmbud.Infrastructure \
  --startup-project src/TaxOmbud.API

# Apply migrations manually
dotnet ef database update \
  --project src/TaxOmbud.Infrastructure \
  --startup-project src/TaxOmbud.API
```

---

## License

Proprietary — Nanro Technology. All rights reserved.
