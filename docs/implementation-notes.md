# Customer API Implementation Notes

## Overview

This document outlines the implementation of the Customer API endpoints for the Loyalty System. The implementation follows the RESTful API design and includes endpoints for user authentication, loyalty program management, loyalty cards, transactions, and rewards.

## Controllers Implemented

### 1. AuthController

The AuthController handles user authentication and user profile management:

- **POST /api/auth/register** - Registers a new customer account
- **POST /api/auth/login** - Authenticates a customer and provides an access token
- **GET /api/auth/loyalty-summary** - Returns a summary of the customer's loyalty programs, points, and rewards

### 2. LoyaltyProgramsController

The LoyaltyProgramsController handles operations related to loyalty programs:

- **GET /api/loyaltyPrograms** - Returns a list of available loyalty programs
- **GET /api/loyaltyPrograms/{id}** - Returns details of a specific loyalty program
- **GET /api/loyaltyPrograms/brand/{brandId}** - Returns loyalty programs for a specific brand
- **GET /api/loyaltyPrograms/nearby** - Returns nearby loyalty programs based on geolocation
- **GET /api/loyaltyPrograms/search** - Searches for loyalty programs

### 3. LoyaltyCardsController

The LoyaltyCardsController handles operations related to a customer's loyalty cards:

- **POST /api/loyaltyCards** - Enrolls a customer in a loyalty program
- **GET /api/loyaltyCards/mine** - Returns the customer's loyalty cards
- **GET /api/loyaltyCards/{id}** - Returns details of a specific loyalty card
- **GET /api/loyaltyCards/{id}/transactions** - Returns transactions for a specific loyalty card
- **GET /api/loyaltyCards/{id}/qr-code** - Returns a QR code for a loyalty card
- **GET /api/loyaltyCards/nearby-stores** - Returns nearby stores for the customer's loyalty cards

### 4. TransactionsController

The TransactionsController handles recording transactions:

- **POST /api/transactions** - Records a new transaction for the authenticated customer

### 5. RewardsController

The RewardsController handles reward redemptions:

- **POST /api/rewards/redeem** - Redeems a reward for the authenticated customer

## Implementation Details

### Authentication Flow

1. Customer registers an account using `/api/auth/register`
2. Customer logs in using `/api/auth/login` and receives an access token
3. The access token is used for all authenticated requests

### Transaction Recording Flow

1. Customer authenticates with the API
2. Customer selects a loyalty card
3. Customer records a transaction with their loyalty card using `/api/transactions`
4. The transaction is processed, and points/stamps are added to the loyalty card

### Reward Redemption Flow

1. Customer authenticates with the API
2. Customer selects a reward to redeem
3. Customer redeems the reward using `/api/rewards/redeem`
4. The system verifies eligibility, deducts points/stamps, and generates a redemption code

## Service Interfaces

The controllers rely on the following service interfaces:

- **ILoyaltyRewardsService** - Handles reward operations like fetching rewards and redeeming rewards
- **ITransactionService** - Handles recording and retrieving transactions
- **ILoyaltyCardService** - Handles loyalty card operations
- **ILoyaltyProgramService** - Handles loyalty program operations

## Data Transfer Objects (DTOs)

- **RegisterUserDto** - Used for customer registration
- **LoginDto** - Used for customer login
- **TransactionDto** - Represents a transaction
- **RecordTransactionDto** - Used for recording a transaction
- **RewardDto** - Represents a reward
- **RedeemRewardDto** - Used for redeeming a reward
- **RewardRedemptionDto** - Represents a reward redemption

## Pagination

Endpoints that return collections support pagination through the `PagedResult<T>` class, which includes:

- **Items** - The items in the current page
- **CurrentPage** - The current page number
- **PageSize** - The number of items per page
- **TotalItems** - The total number of items across all pages
- **TotalPages** - The total number of pages
- **HasPreviousPage** - Whether there is a previous page
- **HasNextPage** - Whether there is a next page

## Error Handling

All endpoints return appropriate HTTP status codes and error messages:

- **200 OK** - Request successful
- **201 Created** - Resource created successfully
- **400 Bad Request** - Invalid request parameters
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Permission denied
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error 