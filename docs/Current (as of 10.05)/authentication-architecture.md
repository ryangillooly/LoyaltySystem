# Authentication Architecture (as of 10/05)

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
  - JWT-based for session management
  - Profile management (view/update own info)
- **Endpoints:**
  - `/api/auth/register`
  - `/api/auth/login`
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
  - JWT-based for session management
  - Profile management (view/update own info)
  - Role-based access (e.g., `Admin`, `BrandManager`, `SuperAdmin`)
- **Endpoints:**
  - `/api/admin/auth/login`
  - `/api/admin/auth/profile` (GET/PUT)
  - `/api/admin/auth/register` (for SuperAdmin to create new admins/managers)
- **Roles:**
  - `Admin`, `BrandManager`, `SuperAdmin`
- **Notes:**
  - Only SuperAdmin can register new admins/managers
  - No public registration

---

## 3. Summary Table: Where Should Auth Live?

| API Route Prefix   | Who Uses It?         | Endpoints (examples)                | Registration? | Login? | Profile? | Roles Enforced? |
|--------------------|----------------------|-------------------------------------|---------------|--------|----------|-----------------|
| `/api/auth/`       | Customers            | register, login, profile            | Yes           | Yes    | Yes      | Customer        |
| `/api/staff/auth/` | Store staff          | login, profile                      | No            | Yes    | Yes      | Staff, Manager  |
| `/api/admin/auth/` | Brand admins/managers| login, profile, register (admin)    | No*           | Yes    | Yes      | Admin, SuperAdmin, BrandManager |

\* Registration only for SuperAdmin to create new admins/managers.

---

## 4. Best Practices & Rationale

- **Separation of Concerns:** Each app's API only exposes auth endpoints relevant to its user base.
- **Security:**
  - No public registration for staff/admins.
  - Role-based JWTs for all.
  - Profile endpoints only allow self-management.
- **Maintainability:**
  - No duplicate endpoints.
  - Each API surface is clear and minimal.

---

## 5. What to Avoid

- No staff/admin registration in the customer API.
- No customer login in the staff/admin APIs.
- No "get user by ID" for customers.
- No duplicate login/register endpoints.

---

## 6. Example API Structure

```
/api/auth/...
    - register (POST)
    - login (POST)
    - profile (GET/PUT)

/api/staff/auth/...
    - login (POST)
    - profile (GET/PUT)

/api/admin/auth/...
    - login (POST)
    - profile (GET/PUT)
    - register (POST, SuperAdmin only)
```

---

## 7. Next Steps

1. Audit your current endpoints against this structure.
2. Remove or move any endpoints that don't fit this model.
3. Update documentation to reflect the new, clean structure.
4. Enforce roles in each API as appropriate. 