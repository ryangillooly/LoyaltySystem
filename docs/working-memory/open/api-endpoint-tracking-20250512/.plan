# API Endpoint Tracking & Progress

**Date:** 2025-05-12 04:28

## Summary
This document tracks the implementation status of all controller endpoints across the Admin, Customer, and Staff APIs, comparing them to the planned architecture. It highlights gaps, outdated routes, and progress for ongoing refactoring and feature work.

---

## Admin API Endpoints
| Route | HTTP Method | Controller | Status | Notes |
|-------|-------------|------------|--------|-------|
| /api/auth/login | POST | AuthController | Implemented | Auth, password, email flows refactored |
| /api/auth/register | POST | AuthController | Implemented | Unified registration |
| /api/auth/profile | GET | AuthController | Implemented | Profile retrieval |
| /api/auth/forgot-password | POST | AuthController | Implemented | Password reset |
| /api/auth/reset-password | POST | AuthController | Implemented | Password reset |
| /api/auth/social-login | POST | AuthController | Implemented | Social login |
| /api/auth/users/{userId}/roles | POST | AuthController | Implemented | Add role |
| /api/auth/users/{userId}/roles | DELETE | AuthController | Implemented | Remove role |
| /api/auth/users/link-customer | POST | AuthController | Implemented | Link customer |
| /api/brands | GET | BrandsController | Implemented | List brands |
| /api/brands/{id} | GET | BrandsController | Implemented | Get brand |
| /api/brands | POST | BrandsController | Implemented | Create brand |
| /api/brands/{id} | PUT | BrandsController | Implemented | Update brand |
| /api/businesses | GET | BusinessesController | Implemented | List businesses |
| /api/businesses/{id} | GET | BusinessesController | Implemented | Get business |
| /api/businesses/{id}/detail | GET | BusinessesController | Implemented | Get business detail |
| /api/businesses/search | GET | BusinessesController | Implemented | Search businesses |
| /api/businesses | POST | BusinessesController | Implemented | Create business |
| /api/businesses/{id} | PUT | BusinessesController | Implemented | Update business |
| /api/businesses/{id} | DELETE | BusinessesController | Implemented | Delete business |
| /api/businesses/{id}/overview | GET | BusinessesController | Implemented | Business overview |
| /api/businesses/{id}/performance | GET | BusinessesController | Implemented | Business performance |
| /api/customers | GET | CustomersController | Implemented | List customers |
| /api/customers/{id} | GET | CustomersController | Implemented | Get customer |
| /api/customers/search | GET | CustomersController | Implemented | Search customers |
| /api/customers | POST | CustomersController | Implemented | Create customer |
| /api/customers/{id} | PUT | CustomersController | Implemented | Update customer |
| /api/customers/{id}/cards | GET | CustomersController | Implemented | Get customer cards |
| /api/customers/{id}/enroll | POST | CustomersController | Implemented | Enroll customer |
| /api/customers/analytics/signups | GET | CustomersController | Implemented | Signup analytics |
| /api/customers/analytics/demographics | GET | CustomersController | Implemented | Demographics |
| /api/customers/analytics/loyalty | GET | CustomersController | Implemented | Loyalty analytics |
| /api/loyalty-programs | GET | LoyaltyProgramsController | Implemented | List programs |
| /api/loyalty-programs/{id} | GET | LoyaltyProgramsController | Implemented | Get program |
| /api/loyalty-programs | POST | LoyaltyProgramsController | Implemented | Create program |
| /api/loyalty-programs/{id} | PUT | LoyaltyProgramsController | Implemented | Update program |
| /api/loyalty-programs/{id} | DELETE | LoyaltyProgramsController | Implemented | Delete program |
| /api/loyalty-programs/{programId}/rewards | GET | RewardsController | Implemented | List rewards |
| /api/loyalty-programs/{programId}/rewards/active | GET | RewardsController | Implemented | List active rewards |
| /api/loyalty-programs/{programId}/rewards/{id} | GET | RewardsController | Implemented | Get reward |
| /api/loyalty-programs/{programId}/rewards/{id}/analytics | GET | RewardsController | Implemented | Reward analytics |
| /api/loyalty-programs/{programId}/rewards/analytics | GET | RewardsController | Implemented | Program rewards analytics |
| /api/loyalty-programs/{programId}/rewards | POST | RewardsController | Implemented | Create reward |
| /api/loyalty-programs/{programId}/rewards/{id} | PUT | RewardsController | Implemented | Update reward |
| /api/stores | GET | StoresController | Implemented | List stores |
| /api/stores/{id} | GET | StoresController | Implemented | Get store |
| /api/stores/brand/{brandId} | GET | StoresController | Implemented | Get stores by brand |
| /api/stores | POST | StoresController | Implemented | Create store |
| /api/stores/{id} | PUT | StoresController | Implemented | Update store |
| /api/loyalty-cards | GET | LoyaltyCardsController | Implemented | List loyalty cards |
| /api/loyalty-cards/{id} | GET | LoyaltyCardsController | Implemented | Get loyalty card |
| /api/loyalty-cards/customer/{customerId} | GET | LoyaltyCardsController | Implemented | Get cards by customer |
| /api/loyalty-cards/program/{programId} | GET | LoyaltyCardsController | Implemented | Get cards by program |
| /api/loyalty-cards | POST | LoyaltyCardsController | Implemented | Create loyalty card |
| /api/loyalty-cards/{id}/status | PUT | LoyaltyCardsController | Implemented | Update card status |
| /api/loyalty-cards/analytics/status | GET | LoyaltyCardsController | Implemented | Card count by status |
| /api/loyalty-cards/analytics/program/{programId} | GET | LoyaltyCardsController | Implemented | Program card analytics |

---

## Customer API Endpoints
| Route | HTTP Method | Controller | Status | Notes |
|-------|-------------|------------|--------|-------|
| /api/auth/login | POST | AuthController | Implemented | Auth, password, email flows refactored |
| /api/auth/register | POST | AuthController | Implemented | Unified registration |
| /api/auth/profile | GET | AuthController | Implemented | Profile retrieval |
| /api/auth/forgot-password | POST | AuthController | Implemented | Password reset |
| /api/auth/reset-password | POST | AuthController | Implemented | Password reset |
| /api/auth/social-login | POST | AuthController | Implemented | Social login |
| /api/loyalty-programs | GET | LoyaltyProgramsController | Implemented | List programs |
| /api/loyalty-programs/{id} | GET | LoyaltyProgramsController | Implemented | Get program |
| /api/loyalty-programs/by-brand/{brandId} | GET | LoyaltyProgramsController | Implemented | Get programs by brand |
| /api/loyalty-programs/{id}/rewards | GET | LoyaltyProgramsController | Implemented | Get rewards for program |
| /api/loyalty-programs/{id}/rewards/{rewardId} | GET | LoyaltyProgramsController | Implemented | Get specific reward |
| /api/loyalty-programs/nearby | GET | LoyaltyProgramsController | Implemented | Get nearby programs |
| /api/loyalty-programs/search | GET | LoyaltyProgramsController | Implemented | Search programs |
| /api/loyalty-cards/mine | GET | LoyaltyCardsController | Implemented | Get my cards |
| /api/loyalty-cards/{id}/transactions | GET | LoyaltyCardsController | Implemented | Get card transactions |
| /api/loyalty-cards/enroll | POST | LoyaltyCardsController | Implemented | Enroll in program |
| /api/loyalty-cards/{id}/qr-code | GET | LoyaltyCardsController | Implemented | Get card QR code |
| /api/loyalty-cards/nearby-stores | GET | LoyaltyCardsController | Implemented | Get nearby stores |
| /api/loyalty-cards/{id} | GET | LoyaltyCardsController | Implemented | Get card by ID |
| /api/loyalty-cards/customer/{customerId} | GET | LoyaltyCardsController | Implemented | Get cards by customer |
| /api/loyalty-cards/program/{programId} | GET | LoyaltyCardsController | Implemented | Get cards by program |
| /api/rewards/redeem | POST | RewardsController | Implemented | Redeem reward |
| /api/customer/profile | GET | CustomerController | Implemented | Get customer profile |
| /api/customer/profile | PUT | CustomerController | Implemented | Update customer profile |
| /api/customer/{id} | PUT | CustomerController | Implemented | Admin update customer |
| /api/customer/link/{customerId} | POST | CustomerController | Implemented | Link customer to user |
| /api/customer/cards | GET | CustomerController | Planned | Needs implementation |

---

## Staff API Endpoints
| Route | HTTP Method | Controller | Status | Notes |
|-------|-------------|------------|--------|-------|
| /api/auth/login | POST | AuthController | Implemented | Staff login |
| /api/auth/profile | GET | AuthController | Implemented | Staff profile |
| /api/store/current | GET | StoreController | Implemented | Get current store |
| /api/store/current/programs | GET | StoreController | Implemented | Get store programs |
| /api/store/current/transactions | GET | StoreController | Implemented | Get store transactions |
| /api/store/current/stats | GET | StoreController | Implemented | Get store stats |
| /api/store/current/hours | GET | StoreController | Implemented | Get store hours |
| /api/loyaltycards/qr/{qrCode} | GET | LoyaltyCardsController | Implemented | Get card by QR |
| /api/loyaltycards/issue-stamps | POST | LoyaltyCardsController | Implemented | Issue stamps |
| /api/loyaltycards/add-points | POST | LoyaltyCardsController | Implemented | Add points |
| /api/loyaltycards/redeem-reward | POST | LoyaltyCardsController | Implemented | Redeem reward |
| /api/loyaltycards | POST | LoyaltyCardsController | Implemented | Create loyalty card |

---

## Next Steps
- [ ] Review all endpoints for completeness and accuracy
- [ ] Mark any outdated or missing endpoints
- [ ] Update architecture docs as new endpoints are added
- [ ] Ensure all controllers are documented in /docs/features and /docs/4. API Architecture
- [ ] Add test coverage for new/changed endpoints

## Current Status

### 2025-05-12 04:28

**Status**: In Progress
- What's working: All major auth, password, and email endpoints refactored and unified. Most business, loyalty, and customer endpoints implemented.
- What's not: Some customer controller endpoints (e.g., /api/customer/cards) need implementation. Ongoing review for completeness.
- Blocking issues: None
- Next actions: Continue endpoint review, update docs, implement missing endpoints.
- Documentation updates needed:
  - [x] Feature documentation changes
  - [x] API documentation updates
  - [ ] Component documentation revisions
  - [ ] Test documentation updates
  - [x] Architecture documentation changes

## Progress History

### 2025-05-12 04:28 - Initial Endpoint Audit
- ✓ Completed: Created comprehensive endpoint tracking table for Admin, Customer, and Staff APIs
- 🤔 Decisions: Unified registration and auth flows, removed redundant endpoints
- 📚 Documentation: Created this tracking doc, to be updated as endpoints evolve
- ⏭️ Led to: Ongoing endpoint review and documentation alignment 