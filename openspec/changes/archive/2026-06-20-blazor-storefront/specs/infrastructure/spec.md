# Delta for Infrastructure

## MODIFIED Requirements

### Requirement: Solution Structure

The system MUST provide a solution named `ShopEaseApp.sln` with one API project and one test project. The system SHALL organize business capabilities as vertical slices under `src/ShopEaseApp.Api/Features/{FeatureName}/`, and shared technical concerns under `src/ShopEaseApp.Api/Infrastructure/`. The API project MUST also host Blazor Server components under `Components/` and feature-specific component folders under `Features/{FeatureName}/Components/`. The project SDK MUST include `Microsoft.AspNetCore.Components.Server`.
(Previously: Solution structure covered only API and test projects with vertical slices — no Blazor component hosting.)

#### Scenario: Required project layout exists

- GIVEN the repository is initialized
- WHEN the solution structure is inspected
- THEN `src/ShopEaseApp.Api` and `tests/ShopEaseApp.Tests` exist
- AND feature and infrastructure boundaries are separated as defined
- AND `Components/` contains Blazor root components (`App.razor`, `Routes.razor`, layouts)
- AND `Microsoft.AspNetCore.Components.Server` is referenced in the API project

### Requirement: Interactive API Documentation

The system MUST expose interactive API documentation through Scalar at `/scalar/v1`. The server MUST serve Razor components with a layout at the root path (`/`) displaying the storefront. API endpoints under `/api/` MUST continue to work alongside Blazor routes without conflict.
(Previously: Only Scalar documentation was required — no storefront serving requirement.)

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
