# Orders Specification

## Purpose

Define checkout and order visibility behavior for customers and administrators.

## Requirements

### Requirement: Create Order From Cart

The system MUST allow an authenticated Customer to create an order from their current cart. The created order MUST include an ID, customer reference, items, unit prices, total, status, and created date. In v1, a successful order SHALL move directly to `Confirmed`.

#### Scenario: Customer checks out successfully
- GIVEN an authenticated Customer has a non-empty cart with available stock
- WHEN the Customer creates an order from the cart
- THEN the system creates an order with status `Confirmed` and the correct total
- AND the cart is cleared

#### Scenario: Anonymous user attempts checkout
- GIVEN the requester is not authenticated as a Customer
- WHEN the requester creates an order from a cart
- THEN the system MUST reject the request
- AND no order is created

### Requirement: Show Order Summary

The system MUST provide an order summary view showing the order ID, customer reference, items, quantities, unit prices, total, status, and created date. A Customer SHALL access only their own orders.

#### Scenario: Customer views owned order summary
- GIVEN an authenticated Customer has a previously created order
- WHEN the Customer requests that order summary
- THEN the system returns the full order summary
- AND the summary reflects the stored order status and totals

#### Scenario: Customer requests another customer's order
- GIVEN an authenticated Customer requests an order they do not own
- WHEN the system evaluates the request
- THEN the system MUST deny access
- AND no order details are disclosed

### Requirement: Show Customer Order History

The system MUST let an authenticated Customer view their own order history. The returned history SHALL include only that Customer's orders.

#### Scenario: Customer views history with orders
- GIVEN an authenticated Customer has one or more orders
- WHEN the Customer requests order history
- THEN the system returns that Customer's orders
- AND each entry includes summary information for review

#### Scenario: Customer views history with no orders
- GIVEN an authenticated Customer has no orders
- WHEN the Customer requests order history
- THEN the system returns an empty result
- AND the request is treated as successful

### Requirement: Show Admin Order List

The system MUST let an authenticated Admin view all orders across customers for operational review.

#### Scenario: Admin views all orders
- GIVEN an authenticated Admin requests the order list
- WHEN the system processes the request
- THEN the system returns orders from all customers
- AND each order includes customer and status summary data

#### Scenario: Customer requests admin order list
- GIVEN an authenticated Customer requests the full order list
- WHEN the system evaluates the request
- THEN the system MUST reject the request
- AND only Admin access is permitted

### Requirement: Reject Order When Stock Is Insufficient

The system MUST reject order creation when any requested variant quantity exceeds available stock at order time. On rejection, the system SHALL NOT confirm an order, decrement stock, or clear the cart.

#### Scenario: Checkout is rejected for insufficient stock
- GIVEN an authenticated Customer has a cart containing a variant with insufficient stock
- WHEN the Customer creates an order from the cart
- THEN the system MUST reject the order
- AND the cart remains unchanged

#### Scenario: Mixed cart has one unavailable variant
- GIVEN an authenticated Customer has a cart with multiple variants and at least one lacks sufficient stock
- WHEN the Customer creates an order from the cart
- THEN the system SHALL reject the entire order
- AND no stock is decremented for any cart item
