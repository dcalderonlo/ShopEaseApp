# Admin Dashboard Specification

## Purpose

Provide admins with inventory health metrics, a product browsing table with stock-status filtering, and inline edit/delete modals for catalog management.

## Requirements

### Requirement: Admin Dashboard Metrics

The system MUST return total product count, total SKU count, low-stock item count, and inventory valuation. The endpoint SHALL require the Admin role.

#### Scenario: Admin retrieves dashboard metrics

- GIVEN products and variants exist with varying stock levels
- WHEN an authenticated Admin requests `GET /api/admin/dashboard`
- THEN the system returns accurate totals for products, SKUs, low-stock items, and inventory value

#### Scenario: Non-admin requests dashboard

- GIVEN the requester lacks the Admin role
- WHEN the requester calls `GET /api/admin/dashboard`
- THEN the system denies access with 403 Forbidden

### Requirement: Admin Product List

The system MUST return all products with variant-level stock, computed status, and price. The endpoint SHALL support filtering by stock status (All, In Stock, Low Stock, Out of Stock).

#### Scenario: Admin browses product list

- GIVEN products exist across stock statuses
- WHEN an Admin requests `GET /api/admin/products`
- THEN the system returns each product with variant stock, computed status badge, and price

#### Scenario: Admin filters by stock status

- GIVEN products exist in multiple stock statuses
- WHEN an Admin requests the list with `?status=LowStock`
- THEN the system returns only products whose variants have Low Stock status

### Requirement: Product Edit via Admin

The system SHALL allow an Admin to update product name, description, category, and image URLs through a modal form.

#### Scenario: Admin edits product details

- GIVEN an Admin opens the edit modal for an existing product
- WHEN the Admin submits valid name, description, category, and image URL changes
- THEN the system persists the updated product fields

#### Scenario: Edit targets non-existent product

- GIVEN the Admin attempts to edit a product ID that does not exist
- WHEN the edit request is submitted
- THEN the system indicates the product was not found

### Requirement: Product Delete via Admin

The system SHALL allow an Admin to delete a product with explicit confirmation. Deletion SHALL cascade to all associated variants.

#### Scenario: Admin deletes a product

- GIVEN an Admin confirms deletion of an existing product
- WHEN the delete request is executed
- THEN the product and all its variants are removed from the catalog

#### Scenario: Delete targets non-existent product

- GIVEN the Admin attempts to delete a product ID that does not exist
- WHEN the delete request is submitted
- THEN the system indicates the product was not found

### Requirement: Stock Status Computation

The system SHALL compute variant stock status as follows: "In Stock" when Stock > MinimumStockLevel, "Low Stock" when 0 < Stock ≤ MinimumStockLevel, "Out of Stock" when Stock = 0.

#### Scenario: Status computed correctly across thresholds

- GIVEN a variant with MinimumStockLevel = 5
- WHEN Stock = 10, Stock = 3, and Stock = 0 are evaluated
- THEN statuses are "In Stock", "Low Stock", and "Out of Stock" respectively

#### Scenario: MinimumStockLevel varies per variant

- GIVEN two variants of the same product with different MinimumStockLevel values
- WHEN both have Stock = 4
- THEN each variant's status reflects its own MinimumStockLevel threshold
