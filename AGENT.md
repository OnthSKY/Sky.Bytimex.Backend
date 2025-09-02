# 🧠 Project Agent Configuration

## 📆 Project Type

* Enterprise-grade **B2B backend service**
* Developed using **.NET 8.0 (C#)** with PostgreSQL
* Clean, layered, and domain-oriented architecture
* Frontend (if applicable) built using **React + TypeScript + Next.js**
* Designed for multi-tenant B2B commerce logic, vendor integrations, and multilingual marketplace

---

## 🔱 Folder Structure

```
/Backend/
├── Application/        → Business logic (services, helpers)
├── Core/               → Shared helpers, constants, enums
├── Contract/           → Request/response DTOs and configs
├── Infrastructure/     → Data access, entities, repositories
├── WebAPI/             → Controllers and API setup

/Tests/
├── Services/           → Unit tests for services
├── Controllers/        → Integration tests for endpoints
```

---

## 🔐 Backend Architecture

### 🚀 Caching Strategy

* Uses **Redis** as distributed cache provider
* Frequently accessed data (e.g. lookup lists, permission maps, user info) is cached
* Caching is implemented declaratively using `[Cacheable]` attribute
* Cached entries are automatically invalidated or refreshed based on business triggers or TTL

### ✅ Core Patterns & Principles

* Follows **SOLID** and **DRY** principles across all layers
* Implements a **generic `IRepository<T>`** pattern
* Uses **Autofac** for dependency injection
* All services are designed with **constructor-based injection** and registered by convention
* Uses **aspect-oriented programming** (AOP) via attributes:

  * `[ValidationAspect]` → FluentValidation-based input validation
  * `[HasPermission]` → Authorization check
  * `[Cacheable]` → Cross-cutting cache layer

### ❗ Exception Handling

* Centralized `GlobalExceptionHandler` middleware
* Catches and logs all unhandled exceptions
* Returns structured error responses in JSON format
* Stack traces are suppressed in production mode

### 🔄 Multilingual Support

* Translation tables store multilingual data (e.g. `product_translations`)
* Resolved based on the `Accept-Language` header per request

### 🕹️ Authentication

* Uses **JWT** tokens for stateless authentication
* JWT contains user ID, role(s), and claims
* Silent login support: If a valid refresh token is present, a new access token is issued without requiring credentials

---

### 🔐 Authorization & User Context
All Create, Update, and Delete operations must extract the authenticated user's ID from the JWT via HttpContext.

The user ID is resolved from the ClaimTypes.NameIdentifier claim.

This value is used to populate the corresponding metadata fields:

created_by, updated_by, deleted_by

The Read operation does not require user context unless filtered by user-specific data.

This mechanism ensures traceability and auditability of user actions across all services.

⚙️ This behavior should be applied consistently across all modules and handled via reusable helper methods.

---

## 🔯 Grid Filtering & Pagination

* Endpoints use a base `GridRequest` object
* Dynamic filters are resolved from a config object per entity/module
* Uses `SqlBuilder` and `GridQueryBuilder` utilities

---

## 🔢 Database Design

* PostgreSQL with `sys` schema
* ENUM types used for `status`, `kyc_status`, etc.
* `created_at`, `updated_at`, `deleted_at` used consistently

---

## 🔮 Testing Strategy

* All services and controllers are covered with **xUnit** tests
* Mocks use **Moq** or internal fake services
* Tests are structured using **Arrange–Act–Assert** pattern

---

## 🔰 Codex Instructions

* See \[Scaffold Missing Modules Prompt] section for table-wise generation
* Ensure entity–DTO parity and update mappings accordingly
* Add `[HasPermission]`, `[ValidationAspect]`, `[Cacheable]` attributes where needed

---

## 🌐 API Design & Conventions

* RESTful endpoint naming (`GET /products`, `POST /orders`, etc.)
* API versioning: `api/[resource]/v{version}`
* All responses follow `ApiResponse<T>` structure
* Sensitive data (e.g. passwords, tokens) must not appear in responses
* Role/permission-based access control via attributes

---

## 📈 Developer Notes

* Use `async` suffix for async methods
* Define new modules by:

  * Adding a resource in `sys.resources`
  * Creating permissions in `sys.permissions`
  * Mapping them via `role_permissions`
* Avoid logic in controllers — only coordination
* Use `AuditLogHelper.LogAction()` for custom logging
* Avoid direct Redis usage, prefer `[Cacheable]`
* Always include DTO validation using FluentValidation and `[ValidationAspect]`
