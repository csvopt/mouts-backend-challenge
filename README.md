# Developer Store Sales API

REST API developed for the Developer Evaluation challenge. It manages the full
lifecycle of sales, applies quantity-based discounts, persists data in
PostgreSQL and publishes sale lifecycle events through Rebus.

## Implemented features

- Complete sales CRUD
- Sale and individual item cancellation
- Quantity-based discounts calculated by the domain
- External identities with denormalized descriptions for customer, branch and product
- Pagination, filtering, date/amount ranges and multiple-field ordering
- PostgreSQL persistence with EF Core migrations
- CQRS-style use cases with MediatR
- Request validation with FluentValidation
- Object mapping with AutoMapper
- `SaleCreated`, `SaleModified`, `SaleCancelled` and `ItemCancelled` events
- Structured application logs with Serilog
- Swagger/OpenAPI
- Health checks
- Unit and functional tests with xUnit, NSubstitute, Bogus and WebApplicationFactory
- Docker Compose environment

## Business rules

Discounts are applied independently to each product:

| Identical item quantity | Discount |
| ---: | ---: |
| 1-3 | 0% |
| 4-9 | 10% |
| 10-20 | 20% |
| Above 20 | Not allowed |

The item and sale totals are always calculated by the domain:

```text
item total = quantity × unit price × (1 - discount)
sale total = sum of all non-cancelled item totals
```

A product cannot appear more than once in the same sale. This prevents splitting
identical items across lines to bypass discount tiers or the maximum quantity.

## Technology

- .NET 8
- ASP.NET Core
- PostgreSQL 13
- Entity Framework Core
- MediatR
- FluentValidation
- AutoMapper
- Rebus with in-memory transport
- Serilog
- xUnit, FluentAssertions, NSubstitute and Bogus

## Project structure

```text
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain
│   ├── Ambev.DeveloperEvaluation.Application
│   ├── Ambev.DeveloperEvaluation.ORM
│   ├── Ambev.DeveloperEvaluation.WebApi
│   ├── Ambev.DeveloperEvaluation.IoC
│   └── Ambev.DeveloperEvaluation.Common
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit
│   ├── Ambev.DeveloperEvaluation.Functional
│   └── Ambev.DeveloperEvaluation.Integration
├── docker-compose.yml
└── Ambev.DeveloperEvaluation.sln
```

The API and Application layers are organized by feature. Each operation has its
own request or command, validator and handler. Business invariants remain in the
`Sale` aggregate and `SaleItem` entity.

## Run with Docker

Requirements:

- Docker with Docker Compose

From the repository root:

```bash
cd template/backend
docker compose up --build
```

The environment starts:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- Health: `http://localhost:8080/health`

The API waits for PostgreSQL to become healthy and applies EF Core migrations
automatically at startup.

Stop the environment while preserving database data:

```bash
docker compose down
```

To also remove the PostgreSQL volume:

```bash
docker compose down -v
```

## Run locally

Requirements:

- .NET SDK 8
- PostgreSQL

Create a PostgreSQL database and configure the connection string through an
environment variable:

```bash
cd template/backend
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n'
dotnet tool restore
dotnet restore
dotnet tool run dotnet-ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

The development launch profile exposes Swagger at the URL printed by `dotnet run`.

## Tests

Run the complete test suite:

```bash
cd template/backend
dotnet test Ambev.DeveloperEvaluation.sln
```

Run only unit tests:

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit
```

Run the HTTP pipeline functional tests:

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Functional
```

The functional suite uses an isolated in-memory database. It does not require
Docker or a running PostgreSQL instance.

Generate Cobertura coverage files:

```bash
./coverage-report.sh
```

## Sales endpoints

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/sales` | Create a sale |
| `GET` | `/api/sales/{id}` | Retrieve a sale |
| `GET` | `/api/sales` | List sales |
| `PUT` | `/api/sales/{id}` | Replace sale details and items |
| `DELETE` | `/api/sales/{id}` | Permanently delete a sale |
| `PATCH` | `/api/sales/{id}/cancel` | Cancel a sale |
| `PATCH` | `/api/sales/{saleId}/items/{itemId}/cancel` | Cancel one item |

### Create or update payload

```json
{
  "saleNumber": "SALE-2026-0001",
  "date": "2026-07-29T15:00:00Z",
  "customerId": "11111111-1111-1111-1111-111111111111",
  "customerName": "Example Customer",
  "branchId": "22222222-2222-2222-2222-222222222222",
  "branchName": "São Paulo Branch",
  "items": [
    {
      "productId": "33333333-3333-3333-3333-333333333333",
      "productName": "Example Product",
      "quantity": 10,
      "unitPrice": 15.90
    }
  ]
}
```

Discounts, item totals, sale totals and cancellation fields are output-only.

## Pagination, filtering and ordering

List parameters:

| Parameter | Description |
| --- | --- |
| `_page` | Page number, default `1` |
| `_size` | Page size from `1` to `100`, default `10` |
| `_order` | Comma-separated fields with optional `asc` or `desc` |
| `saleNumber` | Exact or wildcard filter |
| `customerName` | Exact or wildcard filter |
| `branchName` | Exact or wildcard filter |
| `isCancelled` | Cancellation status |
| `_minDate`, `_maxDate` | Sale date interval |
| `_minTotalAmount`, `_maxTotalAmount` | Total amount interval |

Supported ordering fields are `saleNumber`, `date`, `customerName`, `branchName`,
`totalAmount` and `isCancelled`.

Examples:

```http
GET /api/sales?_page=1&_size=20&_order=date desc,totalAmount desc
GET /api/sales?customerName=*Customer*
GET /api/sales?saleNumber=SALE-2026*
GET /api/sales?_minDate=2026-01-01&_maxDate=2026-12-31
GET /api/sales?_minTotalAmount=100&_maxTotalAmount=1000
```

`*value*`, `value*` and `*value` represent contains, starts-with and ends-with
filters respectively.

## Events and logging

After persistence succeeds, the Application layer publishes:

- `SaleCreatedEvent`
- `SaleModifiedEvent`
- `SaleCancelledEvent`
- `ItemCancelledEvent`

Rebus uses an in-memory transport because the challenge does not require an
external broker. Every publication is also written to the application log.
When the application is not attached to a debugger, logs are written to the
console and daily files under `logs/`.

## Error responses

Errors use appropriate HTTP status codes:

- `400` for invalid requests
- `401` for authentication errors
- `404` for missing resources
- `409` for business-rule conflicts
- `500` for unexpected failures

Example:

```json
{
  "success": false,
  "message": "Business rule violation.",
  "errors": [
    {
      "error": "BusinessRuleViolation",
      "detail": "It is not possible to sell more than 20 identical items."
    }
  ]
}
```

## Design decisions

- Customer, branch and product IDs are stored together with their descriptions,
  preserving the sale snapshot without coupling to other domains.
- Cancelling a sale is a business operation and preserves its historical total.
- Cancelling an item removes that item from the active sale total.
- `DELETE` remains available because the challenge explicitly requests complete
  CRUD; cancellation is exposed separately for business use.
- Updating a sale replaces its item collection and recalculates all discounts
  and totals in the domain.

Additional ready-to-run requests are available in
`src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.http`.
