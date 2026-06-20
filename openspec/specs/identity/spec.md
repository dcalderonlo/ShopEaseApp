# Identity Specification

## Purpose

Define account access, authentication, and role boundaries for ShopEaseApp users and administrators.

## Requirements

### Requirement: User Registration

The system MUST allow guest users to register an account and SHALL create self-registered accounts with the Customer role only.

#### Scenario: Successful customer registration
- GIVEN a guest provides valid registration data
- WHEN the account is submitted
- THEN the system creates the account
- AND assigns the Customer role only

#### Scenario: Registration with invalid or duplicate credentials
- GIVEN a guest submits invalid data or an existing email
- WHEN the account is submitted
- THEN the system rejects the registration

### Requirement: Dual Authentication Login

The system MUST authenticate valid credentials and SHALL issue both a bearer token and an HttpOnly authenticated cookie for the same session.

#### Scenario: Successful dual-auth login
- GIVEN a registered user provides valid credentials
- WHEN the user logs in
- THEN the system returns a bearer token
- AND sets an HttpOnly authentication cookie

#### Scenario: Login with invalid credentials
- GIVEN a registered user provides invalid credentials
- WHEN the user logs in
- THEN the system denies authentication

### Requirement: Logout and Session Revocation

The system MUST allow an authenticated user to log out and SHALL terminate the active authenticated session for both token-backed and cookie-backed access.

#### Scenario: Successful logout
- GIVEN an authenticated user has an active session
- WHEN the user logs out
- THEN the system ends the session
- AND clears further authenticated access until login occurs again

#### Scenario: Logout without an active session
- GIVEN no active authenticated session exists
- WHEN logout is requested
- THEN the system handles the request without granting access

### Requirement: Role Assignment and Enforcement

The system MUST support exactly two roles, Customer and Admin. The system SHALL restrict self-registration to Customer, and Admin assignment MUST occur only through explicit administrative provisioning.

#### Scenario: Customer access remains limited
- GIVEN a self-registered user authenticated as Customer
- WHEN the user accesses customer-protected capabilities
- THEN access is allowed
- AND admin-only capabilities remain denied

#### Scenario: Unauthorized admin access attempt
- GIVEN an authenticated user without the Admin role
- WHEN the user accesses an admin-only capability
- THEN the system denies the request

### Requirement: Authentication Boundaries for Public and Protected Features

The system MUST allow guest access to public catalog browsing and SHALL require authentication for cart and order capabilities.

#### Scenario: Guest browses public catalog
- GIVEN a guest user is not authenticated
- WHEN the user requests catalog browsing
- THEN the system allows access

#### Scenario: Guest requests protected commerce actions
- GIVEN a guest user is not authenticated
- WHEN the user requests cart or order capabilities
- THEN the system denies the request

### Requirement: Session Renewal Scope

The system SHALL support authenticated access only through the issued login token and cookie for this capability. The system MUST require a new login after session expiration because token refresh is not in scope.

#### Scenario: Re-authentication after expiration
- GIVEN a prior session has expired
- WHEN the user attempts to continue authenticated access
- THEN the system requires the user to log in again

#### Scenario: Refresh-style continuation is unavailable
- GIVEN a client attempts to renew an expired session without logging in
- WHEN renewal is requested
- THEN the system does not continue the authenticated session
