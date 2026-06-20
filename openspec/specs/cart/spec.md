# Cart Specification

## Purpose

The cart capability SHALL let authenticated users manage a personal cart before checkout. The cart SHALL be isolated per user ID, store items by product variant, keep the price snapshot captured when each item is added, persist across browser sessions, and SHALL NOT reserve or decrement stock before order confirmation.

## Requirements

### Requirement: View Cart

The system MUST let an authenticated user view only their own cart. The cart MUST include each item's variant ID, quantity, and stored price snapshot, and MAY be empty.

#### Scenario: View a populated cart

- GIVEN an authenticated user with items in their cart
- WHEN the user requests their cart
- THEN the system returns only that user's cart items
- AND each item includes variant ID, quantity, and price snapshot

#### Scenario: Reject unauthenticated cart access

- GIVEN a request without an authenticated user
- WHEN the request attempts to view a cart
- THEN the system denies access
- AND no cart data is disclosed

### Requirement: Add Item

The system MUST let an authenticated user add a product variant to their cart with a requested quantity. When an item is added, the system MUST store the variant ID, quantity, and current price snapshot, and SHALL NOT reserve or decrement stock.

#### Scenario: Add a variant to the cart

- GIVEN an authenticated user and an existing product variant
- WHEN the user adds the variant with a valid quantity
- THEN the variant is present in that user's cart
- AND the stored line includes the quantity and price snapshot captured at add time

#### Scenario: Reject an invalid add request

- GIVEN an authenticated user
- WHEN the user adds a non-existent variant or a quantity below 1
- THEN the system rejects the request
- AND the cart remains unchanged

### Requirement: Update Item Quantity

The system MUST let an authenticated user change the quantity of an existing cart item. Updating quantity MUST preserve the item's variant ID and stored price snapshot, and SHALL NOT reserve or decrement stock.

#### Scenario: Change quantity for an existing cart item

- GIVEN an authenticated user with a variant already in their cart
- WHEN the user updates that item's quantity to a valid value
- THEN the cart reflects the new quantity
- AND the stored price snapshot remains unchanged

#### Scenario: Reject an invalid quantity update

- GIVEN an authenticated user
- WHEN the user updates a missing cart item or sets quantity below 1
- THEN the system rejects the request
- AND no other cart items are changed

### Requirement: Remove Item

The system MUST let an authenticated user remove a specific product variant from their cart.

#### Scenario: Remove one cart item

- GIVEN an authenticated user with multiple items in their cart
- WHEN the user removes one variant from the cart
- THEN only that variant is removed
- AND the remaining cart items stay unchanged

#### Scenario: Remove a variant that is not present

- GIVEN an authenticated user
- WHEN the user removes a variant that is not in the cart
- THEN the cart remains unchanged
- AND no other cart items are removed

### Requirement: Clear Cart

The system MUST let an authenticated user clear all items from their cart. The system MUST also empty the cart after an order is successfully created from that cart.

#### Scenario: Clear a populated cart

- GIVEN an authenticated user with items in their cart
- WHEN the user clears the cart
- THEN the cart becomes empty
- AND no prior line items remain in the cart

#### Scenario: Clear an already empty cart

- GIVEN an authenticated user with an empty cart
- WHEN the user clears the cart
- THEN the cart remains empty
- AND the operation does not add or modify items

#### Scenario: Empty the cart after order creation

- GIVEN an authenticated user with items in their cart
- WHEN an order is successfully created from that cart
- THEN the cart becomes empty
- AND the user can no longer retrieve the transferred line items from the cart

### Requirement: Persist Cart Across Sessions

The system MUST persist a cart across browser sessions for the same authenticated user until the cart is changed or cleared. Cart persistence MUST be keyed by user ID.

#### Scenario: Restore cart for the same user

- GIVEN an authenticated user previously added items to their cart
- WHEN the user closes and later reopens the browser and authenticates again
- THEN the same cart contents are available to that user
- AND the stored quantities and price snapshots are preserved

#### Scenario: Isolate carts between users

- GIVEN one authenticated user has items in their cart
- WHEN a different authenticated user accesses the application
- THEN the second user does not see the first user's cart
- AND only the second user's own cart state is available
