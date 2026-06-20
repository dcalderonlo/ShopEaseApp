# System Specification: ShopEaseApp

## Overview
ShopEaseApp is a highly scalable, high-performance online store built using ASP.NET Core Minimal APIs in .NET 10+. It utilizes a Vertical Slice Architecture (Feature Folders) to group features by business functionality rather than technical layers.

## Core Stack & Dependencies
- **Runtime**: .NET 10+
- **API Framework**: ASP.NET Core Minimal API with Scalar for interactive documentation
- **Database**: SQL Server
- **ORM**: Entity Framework Core with Fluent API configuration
- **Authentication**: JWT Bearer Tokens coupled with HttpOnly Cookies and ASP.NET Core Identity
- **Caching**: Multi-level caching using In-Memory Cache, ASP.NET Core Output Caching, and Distributed Cache (Redis)
- **Validation**: FluentValidation
- **Resilience**: Polly (Microsoft.Extensions.Http.Resilience)
- **Logging**: Serilog
- **Testing**: xUnit with Coverlet for code coverage

## Architectural Conventions
1. **Vertical Slice Architecture**: Each folder under `Features/` should encapsulate its own requests, handlers, domain logic, EF Core queries, and FluentValidation rules.
2. **Strict TDD**: All development must lead with tests (using xUnit) before implementing code.
3. **Database Migrations**: EF Core migrations should be used exclusively for schema changes.
