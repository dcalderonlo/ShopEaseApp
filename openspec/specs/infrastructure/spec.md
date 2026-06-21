# Infrastructure Specification

## Purpose

Define the required platform foundation for ShopEaseApp so all business capabilities run on a consistent solution structure, data platform, observability, documentation, caching, resilience, and local development environment.

## Requirements

### Requirement: Solution Structure

The system MUST provide a solution named `ShopEaseApp.sln` with one API project and one test project. The system SHALL organize business capabilities as vertical slices under `src/ShopEaseApp.Api/Features/{FeatureName}/`, and shared technical concerns under `src/ShopEaseApp.Api/Infrastructure/`. The API project MUST also host Blazor Server components under `Components/` and feature-specific component folders under `Features/{FeatureName}/Components/`. The project SDK MUST include `Microsoft.AspNetCore.Components.Server`.

#### Scenario: Required project layout exists
- GIVEN the repository is initialized
- WHEN the solution structure is inspected
- THEN `src/ShopEaseApp.Api` and `tests/ShopEaseApp.Tests` exist
- AND feature and infrastructure boundaries are separated as defined
- AND `Components/` contains Blazor root components (`App.razor`, `Routes.razor`, layouts)
- AND `Microsoft.AspNetCore.Components.Server` is referenced in the API project

### Requirement: Entity Framework Configuration

The system MUST use Entity Framework Core with SQL Server. Entity mapping SHALL be defined through Fluent API configuration only, and the system MUST NOT depend on data annotations for persistence rules.

#### Scenario: Persistence configuration follows Fluent API only
- GIVEN the persistence model is reviewed
- WHEN entity configuration is validated
- THEN schema rules are defined through Fluent API
- AND data annotations are not required for mapping behavior

### Requirement: Migration Execution Policy

The system MUST manage schema evolution through EF Core migrations. The system SHALL apply migrations automatically at startup in development environments and MUST require deliberate manual migration execution in production environments.

#### Scenario: Environment-specific migration behavior
- GIVEN an application startup occurs
- WHEN the environment is development
- THEN pending migrations are applied automatically
- AND in production the same change requires manual execution

### Requirement: Structured Logging

The system MUST emit structured logs through Serilog. The system SHALL write logs to the console in development and to file-based sinks in production.

#### Scenario: Logging sink matches environment
- GIVEN the application is running
- WHEN logs are produced in development or production
- THEN Serilog captures structured events
- AND the active sink matches the current environment

### Requirement: Interactive API Documentation

The system MUST expose interactive API documentation through Scalar at `/scalar/v1`. The server MUST serve Razor components with a layout at the root path (`/`) displaying the storefront. API endpoints under `/api/` MUST continue to work alongside Blazor routes without conflict.

#### Scenario: Scalar endpoint is available
- GIVEN the API is running
- WHEN `/scalar/v1` is requested
- THEN interactive API documentation is served

#### Scenario: Storefront and API coexist
- GIVEN the application is running with Blazor Server enabled
- WHEN `/` is requested
- THEN the storefront Razor layout is rendered
- AND WHEN `/api/products` is requested
- THEN the API endpoint returns JSON responses normally

### Requirement: Layered Caching Strategy

The system MUST support multiple cache layers with explicit scope boundaries. Public catalog endpoints SHALL use Output Cache, reference data such as categories SHALL use In-Memory Cache, and cart or session data MUST use Redis Distributed Cache.

#### Scenario: Cache layer matches data type
- GIVEN catalog, category, and cart data are requested
- WHEN caching behavior is evaluated
- THEN public catalog responses use HTTP output caching
- AND categories use process-level memory caching
- AND cart or session state uses distributed Redis storage

### Requirement: Resilience Policies

The system MUST apply Polly-based resilience to external dependencies. Outbound HTTP calls SHALL use retry with exponential backoff, and Redis dependency failures SHALL be protected by a circuit breaker policy.

#### Scenario: Dependency failures trigger resilience rules
- GIVEN an outbound dependency becomes unstable
- WHEN HTTP or Redis failures occur
- THEN HTTP requests retry with backoff
- AND repeated Redis failures open circuit protection

### Requirement: Local Development Dependencies

The system MUST provide a `docker-compose.yml` for local development that provisions Redis and SQL Server services.

#### Scenario: Development services are declared
- GIVEN a developer prepares the local environment
- WHEN the compose definition is reviewed
- THEN Redis and SQL Server services are available for local startup
