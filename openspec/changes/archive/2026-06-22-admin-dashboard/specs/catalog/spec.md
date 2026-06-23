# Delta for Catalog

## MODIFIED Requirements

### Requirement: Manage Product Variants

The system SHALL treat each product variant as a distinct sellable option with its own attributes, price, stock, and minimum stock level.

(Previously: Variants had attributes, price, and stock only — no minimum stock level concept.)

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
