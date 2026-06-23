# Catalog Specification

## Purpose

Define public catalog browsing and admin catalog management for products, categories, variants, pricing, stock, and image references.

## Requirements

### Requirement: List Products

The system SHALL allow Guests and Customers to browse products without authentication and SHOULD support repeatable cached responses without changing visible results.

#### Scenario: Browse available products
- GIVEN public products exist across categories
- WHEN a user requests the product list
- THEN the system returns products with category, image references, and variant summaries

#### Scenario: Browse with no matches
- GIVEN no products satisfy the requested browse criteria
- WHEN a user requests the product list
- THEN the system returns an empty result without requiring authentication

### Requirement: Get Product by ID

The system SHALL allow Guests and Customers to view a single product by identifier without authentication.

#### Scenario: View product details
- GIVEN a product exists
- WHEN a user requests that product by ID
- THEN the system returns name, description, category, images, and variants with price and stock

#### Scenario: Product does not exist
- GIVEN no product exists for the requested ID
- WHEN a user requests that product by ID
- THEN the system indicates the product was not found

### Requirement: Create Product

The system SHALL allow only Admin users to create a product with exactly one category, image references, and at least one variant.

#### Scenario: Admin creates a product
- GIVEN an Admin provides valid product data
- WHEN the product is created
- THEN the system stores the product with its category, images, and variants

#### Scenario: Non-admin attempts creation
- GIVEN the requester is not an Admin
- WHEN the requester submits product creation
- THEN the system denies the operation

### Requirement: Update Product

The system SHALL allow only Admin users to update product details, category assignment, image references, and variants.

#### Scenario: Admin updates a product
- GIVEN an Admin targets an existing product
- WHEN valid changes are submitted
- THEN the system persists the updated product state

#### Scenario: Update targets unknown product
- GIVEN no product exists for the requested ID
- WHEN an Admin submits an update
- THEN the system indicates the product was not found

### Requirement: Delete Product

The system SHALL allow only Admin users to delete a product from the catalog.

#### Scenario: Admin deletes a product
- GIVEN an Admin targets an existing product
- WHEN deletion is requested
- THEN the product is removed from public browsing

#### Scenario: Customer attempts deletion
- GIVEN the requester is not an Admin
- WHEN deletion is requested
- THEN the system denies the operation

### Requirement: List Categories

The system SHALL allow Guests and Customers to browse categories without authentication.

#### Scenario: Browse categories
- GIVEN categories exist for the catalog
- WHEN a user requests the category list
- THEN the system returns the available categories for browsing

#### Scenario: No categories exist
- GIVEN the catalog has no categories
- WHEN a user requests the category list
- THEN the system returns an empty result without error

### Requirement: Manage Categories

The system SHALL allow only Admin users to create, update, and delete categories used by products.

#### Scenario: Admin manages a category
- GIVEN an Admin submits valid category changes
- WHEN the create, update, or delete action is requested
- THEN the system applies the requested category change

#### Scenario: Category deletion would break product assignment
- GIVEN a category is assigned to existing products
- WHEN an Admin requests deletion
- THEN the system rejects the deletion until product assignment is resolved

### Requirement: Manage Product Variants

The system SHALL treat each product variant as a distinct sellable option with its own attributes, price, stock, and minimum stock level.

#### Scenario: Admin manages variants

- GIVEN an Admin edits an existing or new variant
- WHEN valid variant data is submitted
- THEN the system stores the variant with its own price, stock, and MinimumStockLevel under the product

#### Scenario: Variant data is invalid

- GIVEN an Admin submits a variant with missing attributes or invalid price or stock
- WHEN the variant change is requested
- THEN the system rejects the invalid variant change

#### Scenario: Admin creates variant with MinimumStockLevel

- GIVEN an Admin creates a new product variant
- WHEN MinimumStockLevel is provided in the request
- THEN the system stores the specified MinimumStockLevel value
- AND when MinimumStockLevel is omitted, the system defaults it to 5

#### Scenario: VariantSummary includes MinimumStockLevel

- GIVEN a variant exists with a configured MinimumStockLevel
- WHEN the system returns a VariantSummary response
- THEN the response includes the MinimumStockLevel field alongside price and stock
