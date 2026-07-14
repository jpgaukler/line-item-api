# Line Item API

A .NET 10 Web API for managing configurable products and generating priced quotes for them. Products
are defined by a template of **inputs** (form/fit/functionality options) and **adders** (optional extras), each
carrying a price, and the API supports a **draft → publish** workflow so changes can be edited safely before they
become the live, versioned definition of a product.

This project is one of the samples on my resume — it's a small, complete slice of a real domain (draft/publish +
version history + full-text search over Postgres) built with a layered .NET architecture, raw SQL via Dapper, and
FluentMigrator-managed schema/migrations, backed by in-process integration tests.

## What it does

- **Products** are the published, versioned definition of something that can be quoted (e.g., a configurable
  physical product). Every publish creates a new immutable version; the parent `product` row always points at its
  current `active_version`.
- **Product Drafts** are the mutable, in-progress editing surface for a product. A draft can be created from
  scratch or seeded from an existing product's current version, edited, and then **published**, which validates the
  draft and promotes it into a new `product_version`.
- **Product Categories** group products for organization and filtering.
- **Search** supports fuzzy, typo-tolerant lookup over product name/description using Postgres trigram similarity
  (`pg_trgm`).
- **Users** are synced from Auth0 on first authenticated request (see `UserContextMiddleware`) rather than managed
  through a separate signup flow.
- **Quotes** (in `QuoteController`) is a work-in-progress feature for generating PDF quotes from a product
  configuration via Handlebars templating + a Gotenberg HTML-to-PDF service.

## Architecture

The solution is split into small, single-purpose class libraries so that API concerns, business rules, data access,
and schema management can evolve independently:

```
Controllers (LineItem.Api)
        │  HTTP, versioning, auth, request/response shaping
        ▼
Services (LineItem.Services)
        │  business rules, validation, orchestration across repositories
        ▼
Repositories (LineItem.Repositories)
        │  Dapper + Postgres functions, one repository per aggregate
        ▼
Postgres (via LineItem.Migrations)
           schema + PL/pgSQL functions, versioned with FluentMigrator
```

Cross-cutting pieces:

- **LineItem.Models** — plain DTOs/domain models shared between the API and Services layers (e.g. `Product`,
  `ProductDraft`, `UserModel`). No persistence concerns live here.
- **LineItem.Exceptions** — a small set of typed exceptions (`NotFoundException`, `ValidationException`) that
  controllers translate into the right HTTP status codes.
- **LineItem.BuildVersion** — stamps the assembly with build metadata (used in the `/health` response).
- **LineItem.Migrations** — a standalone console app that runs FluentMigrator migrations against Postgres; it is
  deployed and run independently from the API.

Request flow example (`GET /v1/products/{id}`):

1. `ProductController` receives the request and delegates to `IProductService`.
2. `ProductService` (business logic, currently a thin pass-through for reads) calls `IProductRepository`.
3. `ProductRepository` opens a Dapper connection and calls the `lineitem.product_retrieve_active_version_by_id`
   Postgres function.
4. The JSONB `product_data` column is deserialized back into a `Product` and returned up the stack.

Other notable pieces:

- **API versioning** (`Asp.Versioning`) — routes are versioned via URL segment (`v1/...`) or an
  `X-Api-Version` header.
- **Auth0** — bearer-token authentication via `Auth0.AspNetCore.Authentication.Api`; `UserContextMiddleware` maps
  the authenticated Auth0 subject to an internal `app_user` row (creating one on first sight) and caches the
  mapping in memory.
- **Serilog** — structured logging, console output in `local`, compact JSON elsewhere.
- **Health checks** — `/health` reports API build version plus a live Postgres connectivity check
  (`DatabaseHealthCheck`).

## Project / folder structure

```
LineItem.Api/                    # Web API — controllers, middleware, startup, health checks
├── Controllers/                 # One controller per resource
├── Middleware/                  # UserContextMiddleware (Auth0 → app_user)
├── HealthChecks/                # DatabaseHealthCheck
├── Clients/                     # PdfGeneratorClient (Gotenberg)
└── Helpers/                     # Extension helpers

LineItem.Services/                # Business logic, one service per aggregate

LineItem.Repositories/            # Dapper data access, one repository per aggregate
├── Tables/                      # Row classes matching SQL query results
├── Helpers/                     # DI wiring, mapping extensions
└── Options/                     # DatabaseOptions

LineItem.Models/                  # Shared domain models and DTOs
LineItem.Exceptions/              # Custom exceptions for controller error handling
LineItem.BuildVersion/            # Helper project for Build/version stamping

LineItem.Migrations/              # Console app that runs FluentMigrator migrations
├── Migrations/
│   └── {timestamp}_{SchemaName}/
│       ├── {SchemaName}.cs      # Migration class — calls Up()/Down() scripts by name
│       └── Up/*.sql             # Table, constraint, and function scripts
├── Metadata/                    # VersionTableMetadata
└── Helpers/                     # Helper methods for migrations

LineItem.Test.Integration/        # xUnit tests exercising the API end-to-end over HTTP
├── Fixtures/                    # ApiFixture — in-process test server + HttpClient
├── Builders/                    # Test data builders
└── IntegrationTestBase.cs       # Shared setup/cleanup helpers

LineItem.Test.Unit/                # Isolated unit tests (e.g. UserContextMiddleware)
```

## Postgres Setup: Migrations and Functions

Schema changes are managed by **FluentMigrator** (`LineItem.Migrations`), run as a separate console app/step in
the deploy pipeline rather than at API startup, with all business logic pushed into PL/pgSQL functions instead of
inline SQL in C#.

- Each migration is a folder (e.g. `Migrations/202606251708_ProductSchema/`) containing a `{SchemaName}.cs` class
  plus an `Up/*.sql` folder of table, constraint, and function scripts.
- The C# migration class holds no inline SQL — it calls `Execute.Script(this.GetUpScript("file.sql"))` for each
  script in order, so schema and function definitions live in real, reviewable `.sql` files.
- `Down()` reverses those same steps by hand (drop functions → drop constraints → drop tables), giving each
  migration a symmetric rollback path.
- Every repository call hits a Postgres function (e.g. `product_search`, `product_update_active_version`) rather
  than an inline `SELECT`/`INSERT` — this keeps multi-statement writes atomic and keeps the query logic testable
  independent of the C# calling it.

## Dapper: SQL Access Layer

Dapper is used as a thin micro-ORM over `Npgsql` — no EF Core, no change-tracking, no LINQ query translation.

- Each repository inherits from `RepositoryBase`, which owns the connection string and hands back a fresh
  `NpgsqlConnection` per call.
- Queries are built as Dapper `CommandDefinition` and run against Postgres functions, never raw tables.
- Results map onto row classes in `Repositories/Tables/` whose properties mirror the returned columns —
  `DefaultTypeMap.MatchNamesWithUnderscores = true` lets Dapper match `snake_case` columns to `PascalCase`
  properties automatically, no attributes needed.
- JSONB columns (e.g. `product_data`) are deserialized into the public `LineItem.Models` types via small mapping
  extensions.
- Because the real query logic lives in the SQL functions, repositories stay small: build parameters, run the
  command, map the row.

## Integration Tests

`LineItem.Test.Integration` exercises the full stack over real HTTP calls, with nothing mocked.

- `ApiFixture` boots the API in-process via `WebApplicationFactory<Program>`, so tests don't need the API running
  separately.
- Setting an `API_SERVER_URL` environment variable instead points the same suite at a live deployment, doubling it
  as a smoke test.
- `IntegrationTestBase` provides shared setup/cleanup helpers (`CreateTestUserAsync`, `CreateProductDraftAsync`,
  `PublishProductDraftAsync`, ...) that each test uses to arrange its own data and clean it up afterward.
- Tests are black-box — they only call HTTP endpoints, so a pass means controller, service, repository, and
  Postgres function all actually work together.
- `LineItem.Test.Unit` holds separate, genuinely isolated unit tests that don't need a live server or database.

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **PostgreSQL** with **FluentMigrator** for schema management and **Dapper** + **Npgsql** for data access
- **Auth0** for authentication, **Asp.Versioning** for API versioning
- **Serilog** for structured logging
- **xUnit** with `WebApplicationFactory` for in-process integration testing
- **Handlebars.Net** + **Gotenberg** (via a lightweight HTTP client) for HTML-to-PDF quote generation (in progress)
