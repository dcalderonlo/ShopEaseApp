# Blazor Storefront Specification

## Purpose

Define the Blazor Server storefront UI for ShopEaseApp, enabling guest catalog browsing, authenticated cart management, checkout, order history, and identity flows — all hosted within the existing API project.

## Requirements

### Requirement: Public Catalog Browsing

The system MUST render a product listing page and product detail page accessible to unauthenticated users. Categories MUST be browsable via a dedicated page or navigation.

#### Scenario: Guest browses product catalog

- GIVEN a guest user navigates to the storefront root
- WHEN the catalog page loads
- THEN all active products are displayed with name, price, and category
- AND the user can click any product to view its detail page

#### Scenario: Product not found

- GIVEN a guest requests a product detail page
- WHEN the product ID does not exist or is inactive
- THEN a "Product not found" message is displayed
- AND the user is offered a link back to the catalog

### Requirement: Cart Management (Auth Required)

The system MUST allow authenticated users to view their cart, add items, remove items, and update quantities via Blazor UI components. A cart summary MUST be visible in the navbar on every page.

#### Scenario: Authenticated user adds item to cart

- GIVEN an authenticated user views a product detail page
- WHEN the user clicks "Add to Cart"
- THEN the cart summary in the navbar updates with the new item count
- AND the item appears in the cart page with correct quantity and price

#### Scenario: Unauthenticated user attempts cart access

- GIVEN a guest user navigates to the cart page
- WHEN the page loads
- THEN the user is redirected to the login page
- AND after login the user returns to the cart page

### Requirement: Checkout Flow

The system MUST allow authenticated users to proceed from cart to an order confirmation page. The checkout MUST create an order record and clear the cart upon success.

#### Scenario: Successful checkout

- GIVEN an authenticated user has items in their cart
- WHEN the user clicks "Checkout" and confirms
- THEN an order is created and an order confirmation page displays the order number
- AND the cart is cleared

#### Scenario: Empty cart checkout attempt

- GIVEN an authenticated user visits the checkout page with an empty cart
- WHEN the page loads
- THEN the user is redirected to the cart page
- AND a message indicates the cart is empty

### Requirement: Order History

The system MUST display a list of past orders for authenticated users, showing order number, date, status, and total.

#### Scenario: User views order history

- GIVEN an authenticated user navigates to "My Orders"
- WHEN the order history page loads
- THEN all orders placed by the user are listed in descending date order
- AND each entry shows order number, date, status, and total

#### Scenario: No orders yet

- GIVEN an authenticated user with no orders navigates to "My Orders"
- WHEN the page loads
- THEN a message indicates no orders have been placed yet
- AND a link to browse the catalog is provided

### Requirement: Register and Login (Minimal)

The system MUST provide Blazor forms for guest registration and login. Upon successful login, the system MUST set an authentication cookie so subsequent Blazor pages recognize the user.

#### Scenario: Guest registers and logs in

- GIVEN a guest user navigates to the register page
- WHEN the user submits valid registration details
- THEN the account is created and the user is redirected to login
- AND after login the auth cookie is set and the navbar shows the username

#### Scenario: Invalid login credentials

- GIVEN a user submits the login form with incorrect credentials
- WHEN the form is submitted
- THEN an error message is displayed
- AND no auth cookie is set

### Requirement: Navigation Auth State

The system MUST render login and register links in the navbar for guests, and display the authenticated username with a logout link for logged-in users.

#### Scenario: Navbar reflects guest state

- GIVEN no user is authenticated
- WHEN the navbar renders
- THEN "Login" and "Register" links are visible
- AND no username or logout link is shown

#### Scenario: Navbar reflects authenticated state

- GIVEN a user is authenticated
- WHEN the navbar renders
- THEN the username and a "Logout" link are visible
- AND "Login" and "Register" links are hidden
