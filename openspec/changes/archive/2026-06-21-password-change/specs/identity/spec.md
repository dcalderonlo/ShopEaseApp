# Delta for Identity

## ADDED Requirements

### Requirement: Authenticated Password Change

The system MUST allow an authenticated user to change their own password by providing their current password and a new password. The change SHALL fail if the current password is incorrect.

#### Scenario: Successful password change

- GIVEN an authenticated user provides their correct current password and a valid new password
- WHEN the change is submitted
- THEN the system updates the password
- AND the user can login with the new password

#### Scenario: Incorrect current password

- GIVEN an authenticated user provides an incorrect current password
- WHEN the change is submitted
- THEN the system rejects the change
- AND the password remains unchanged

#### Scenario: New password does not meet requirements

- GIVEN an authenticated user submits a new password shorter than 6 characters
- WHEN the change is submitted
- THEN the system rejects the change with a validation error

### Requirement: Forced Password Change on First Login

The system MUST require a password change on first login when an account is flagged with MustChangePassword. The login response SHALL include a flag indicating whether a password change is required.

#### Scenario: Admin flagged for first change is redirected

- GIVEN a seeded admin account with MustChangePassword=true
- WHEN the admin logs in successfully
- THEN the login response indicates MustChangePassword=true
- AND the UI redirects the admin to the change password page

#### Scenario: User not flagged proceeds normally

- GIVEN an existing user without the MustChangePassword flag
- WHEN the user logs in successfully
- THEN the login response indicates MustChangePassword=false
- AND the UI redirects the user to the home page

## MODIFIED Requirements

### Requirement: Dual Authentication Login

The system MUST authenticate valid credentials and SHALL issue both a bearer token and an HttpOnly authenticated cookie for the same session. The login response SHALL include a MustChangePassword boolean indicating whether the user must change their password before accessing protected features.
(Previously: Dual-auth login returned token and cookie without any password-change flag.)

#### Scenario: Successful dual-auth login

- GIVEN a registered user provides valid credentials
- WHEN the user logs in
- THEN the system returns a bearer token
- AND sets an HttpOnly authentication cookie

#### Scenario: Login with invalid credentials

- GIVEN a registered user provides invalid credentials
- WHEN the user logs in
- THEN the system denies authentication

#### Scenario: Login with MustChangePassword flag

- GIVEN an admin account with MustChangePassword=true provides valid credentials
- WHEN the admin logs in
- THEN the system returns a bearer token and sets an HttpOnly cookie
- AND the login response includes MustChangePassword=true
