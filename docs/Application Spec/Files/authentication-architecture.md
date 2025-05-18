# Authentication Architecture (as of 10/05, updated)

## Overview

This document outlines the authentication structure for the Loyalty System as of 10/05, covering the Customer App, Merchant App (Store Staff), and Merchant App (Brand Managers/Admins). It is based on best practices, project rules, and current documentation.

---

## 1. Applications & Audiences

| Application      | Audience/Role(s)                | Typical Use Cases                                 |
|------------------|---------------------------------|---------------------------------------------------|
| Customer App     | End customers                   | Register, login, view/update profile, loyalty     |
| Merchant App     | Store staff (cashiers, managers)| Process transactions, enroll customers, redeem    |
| Merchant App     | Brand managers/admins           | Analytics, configuration, marketing, user mgmt    |

---

## 2. Authentication Models

### A. Customer App
- **Who:** End customers (shoppers)
- **Auth Should Be:**
  - Register (email, password, etc.)
  - Login (email/username + password)
  - Social login (Google, Apple)
  - Password reset (forgot/reset)
  - JWT-based for session management
  - Profile management (view/update own info)
- **Endpoints:**
  - `/api/auth/register` (now supports roles array, but only "Customer" allowed)
  - `/api/auth/login`
  - `/api/auth/social-login` *(new)*
  - `/api/auth/forgot-password` *(new)*
  - `/api/auth/reset-password` *(new)*
  - `/api/auth/profile` (GET/PUT)
- **Roles:**
  - `Customer`

### B. Merchant App (Store Staff)
- **Who:** Store staff (cashiers, store managers)
- **Auth Should Be:**
  - Login (email/username + password)
  - JWT-based for session management
  - Profile management (view/update own info)
  - Role-based access (e.g., `Staff`, `StoreManager`)
- **Endpoints:**
  - `/api/staff/auth/login`
  - `/api/staff/auth/profile` (GET/PUT)
- **Roles:**
  - `Staff`, `StoreManager`
- **Notes:**
  - No registration endpoint (staff are created by brand/merchant admins)
  - Staff cannot self-register

### C. Merchant App (Brand Managers/Admins)
- **Who:** Brand managers, admins, business owners
- **Auth Should Be:**
  - Login (email/username + password)
  - Social login (Google, Apple)
  - Password reset (forgot/reset)
  - JWT-based for session management
  - Profile management (view/update own info)
  - Role-based access (e.g., `Admin`, `BrandManager`, `SuperAdmin`)
- **Endpoints:**
  - `/api/admin/auth/register` (now supports roles array, e.g., ["Admin", "Manager"])
  - `/api/admin/auth/login`
  - `/api/admin/auth/social-login` *(new)*
  - `/api/admin/auth/forgot-password` *(new)*
  - `/api/admin/auth/reset-password` *(new)*
  - `/api/admin/auth/profile` (GET/PUT)
  - `/api/admin/auth/users/{userId}/roles` (add/remove roles)
  - `/api/admin/auth/users/link-customer` (link user to customer)
- **Roles:**
  - `Admin`, `BrandManager`, `SuperAdmin`
- **Notes:**
  - Only SuperAdmin/Admin can register new admins/managers (enforced by roles array and controller logic)
  - No public registration

---

## 3. Summary Table: Where Should Auth Live?

| API Route Prefix   | Who Uses It?         | Endpoints (examples)                | Registration? | Login? | Social Login? | Password Reset? | Profile? | Roles Enforced? |
|--------------------|----------------------|-------------------------------------|---------------|--------|---------------|-----------------|----------|-----------------|
| `/api/auth/`       | Customers            | register, login, social-login, forgot-password, reset-password, profile | Yes           | Yes    | Yes           | Yes             | Yes      | Customer        |
| `/api/staff/auth/` | Store staff          | login, profile                      | No            | Yes    | No            | No              | Yes      | Staff, Manager  |
| `/api/admin/auth/` | Brand admins/managers| register, login, social-login, forgot-password, reset-password, profile, roles mgmt, link-customer | Yes*          | Yes    | Yes           | Yes             | Yes      | Admin, SuperAdmin, BrandManager |

\* Registration only for SuperAdmin/Admin to create new admins/managers, via roles array.

---

## 4. Endpoint Changes (as of this update)

### **Removed Endpoints:**
- `/api/admin/auth/register/admin` *(replaced by unified /register with roles)*
- `/api/admin/auth/register/manager` *(replaced by unified /register with roles)*

### **Added Endpoints:**
- `/api/auth/social-login` *(Customer, Admin)*
- `/api/auth/forgot-password` *(Customer, Admin)*
- `/api/auth/reset-password` *(Customer, Admin)*
- `/api/admin/auth/register` and `/api/auth/register` now accept a `roles` array

---

## 5. Best Practices & Rationale

- **Separation of Concerns:** Each app's API only exposes auth endpoints relevant to its user base.
- **Security:**
  - No public registration for staff/admins.
  - Role-based JWTs for all.
  - Profile endpoints only allow self-management.
  - Social login and password reset flows are secure and provider-validated.
- **Maintainability:**
  - No duplicate endpoints.
  - Each API surface is clear and minimal.

---

## 6. What to Avoid

- No staff/admin registration in the customer API.
- No customer login in the staff/admin APIs.
- No "get user by ID" for customers.
- No duplicate login/register endpoints.
- No public password reset token enumeration.

---

## 7. Example API Structure

```
/api/auth/...
    - register (POST, with roles array)
    - login (POST)
    - social-login (POST)
    - forgot-password (POST)
    - reset-password (POST)
    - profile (GET/PUT)

/api/staff/auth/...
    - login (POST)
    - profile (GET/PUT)

/api/admin/auth/...
    - register (POST, with roles array)
    - login (POST)
    - social-login (POST)
    - forgot-password (POST)
    - reset-password (POST)
    - profile (GET/PUT)
    - users/{userId}/roles (POST/DELETE)
    - users/link-customer (POST)
```

---

## 8. Next Steps

1. Audit your current endpoints against this structure.
2. Remove or move any endpoints that don't fit this model.
3. Update documentation to reflect the new, clean structure.
4. Enforce roles in each API as appropriate. 