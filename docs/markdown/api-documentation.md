# Loyalty System API Documentation

This document provides comprehensive documentation for the Loyalty System APIs, covering all endpoints required for the Customer, Staff, and Admin applications.

## Table of Contents

- [Authentication](#authentication)
  - [Customer Authentication](#customer-authentication)
  - [Admin Authentication](#admin-authentication)
  - [Staff Authentication](#staff-authentication)
- [Customer API](#customer-api)
  - [Loyalty Programs](#loyalty-programs)
  - [Loyalty Cards](#loyalty-cards)
  - [Transactions](#transactions)
  - [Rewards](#rewards)
- [Admin API](#admin-api)
  - [Business Management](#business-management)
  - [Loyalty Program Management](#loyalty-program-management)
  - [Reward Management](#reward-management)
  - [Analytics](#analytics)
- [Staff API](#staff-api)
  - [Customer Management](#customer-management)
  - [Transaction Processing](#transaction-processing)
  - [Reward Redemption](#reward-redemption)

## Authentication

### Customer Authentication

#### Register a new customer account

- **URL**: `/api/auth/register`
- **Method**: `POST`
- **Auth Required**: No
- **Request Body**:
  ```json
  {
    "email": "customer@example.com",
    "password": "SecurePassword123!",
    "confirmPassword": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**:
    ```json
    {
      "id": "user-uuid",
      "email": "customer@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "createdAt": "2023-03-14T12:00:00Z",
      "updatedAt": "2023-03-14T12:00:00Z"
    }
    ```

#### Login to customer account

- **URL**: `/api/auth/login`
- **Method**: `POST`
- **Auth Required**: No
- **Request Body**:
  ```json
  {
    "email": "customer@example.com",
    "password": "SecurePassword123!"
  }
  ```
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "token": "jwt-token",
      "user": {
        "id": "user-uuid",
        "email": "customer@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "createdAt": "2023-03-14T12:00:00Z",
        "updatedAt": "2023-03-14T12:00:00Z"
      },
      "expiration": "2023-03-15T12:00:00Z"
    }
    ```

#### Get customer loyalty summary

- **URL**: `/api/auth/loyalty-summary`
- **Method**: `GET`
- **Auth Required**: Yes
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "customerId": "customer-uuid",
      "activeProgramCount": 2,
      "totalPoints": 150,
      "totalStamps": 8,
      "pendingRewards": 1
    }
    ```

### Admin Authentication

#### Login to admin account

- **URL**: `/api/admin/auth/login`
- **Method**: `POST`
- **Auth Required**: No
- **Request Body**:
  ```json
  {
    "email": "admin@example.com",
    "password": "AdminSecurePassword123!"
  }
  ```
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "token": "jwt-token",
      "user": {
        "id": "admin-uuid",
        "email": "admin@example.com",
        "firstName": "Admin",
        "lastName": "User",
        "createdAt": "2023-03-14T12:00:00Z",
        "updatedAt": "2023-03-14T12:00:00Z"
      },
      "expiration": "2023-03-15T12:00:00Z"
    }
    ```

### Staff Authentication

#### Login to staff account

- **URL**: `/api/staff/auth/login`
- **Method**: `POST`
- **Auth Required**: No
- **Request Body**:
  ```json
  {
    "email": "staff@example.com",
    "password": "StaffSecurePassword123!"
  }
  ```
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "token": "jwt-token",
      "user": {
        "id": "staff-uuid",
        "email": "staff@example.com",
        "firstName": "Staff",
        "lastName": "User",
        "createdAt": "2023-03-14T12:00:00Z",
        "updatedAt": "2023-03-14T12:00:00Z"
      },
      "expiration": "2023-03-15T12:00:00Z"
    }
    ```

## Customer API

### Loyalty Programs

#### Get available loyalty programs

- **URL**: `/api/loyaltyPrograms`
- **Method**: `GET`
- **Auth Required**: Yes
- **Query Parameters**:
  - `page` (optional): Page number (default: 1)
  - `pageSize` (optional): Items per page (default: 20)
  - `brandId` (optional): Filter by brand ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "items": [
        {
          "id": "program-uuid",
          "name": "Coffee Rewards",
          "description": "Earn points with every coffee purchase",
          "businessId": "business-uuid",
          "brandId": "brand-uuid",
          "isActive": true,
          "createdAt": "2023-03-14T12:00:00Z",
          "rewards": []
        }
      ],
      "totalItems": 1,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 1
    }
    ```

#### Get loyalty program by ID

- **URL**: `/api/loyaltyPrograms/{id}`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty program ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "id": "program-uuid",
      "name": "Coffee Rewards",
      "description": "Earn points with every coffee purchase",
      "businessId": "business-uuid",
      "brandId": "brand-uuid",
      "isActive": true,
      "createdAt": "2023-03-14T12:00:00Z",
      "rewards": [
        {
          "id": "reward-uuid",
          "name": "Free Coffee",
          "description": "Get a free coffee of your choice",
          "loyaltyProgramId": "program-uuid",
          "pointsCost": 50,
          "isActive": true,
          "createdAt": "2023-03-14T12:00:00Z",
          "expiresAt": null
        }
      ]
    }
    ```

#### Get loyalty programs by brand ID

- **URL**: `/api/loyaltyPrograms/brand/{brandId}`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `brandId`: The brand ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "program-uuid",
        "name": "Coffee Rewards",
        "description": "Earn points with every coffee purchase",
        "businessId": "business-uuid",
        "brandId": "brand-uuid",
        "isActive": true,
        "createdAt": "2023-03-14T12:00:00Z",
        "rewards": []
      }
    ]
    ```

#### Get nearby loyalty programs

- **URL**: `/api/loyaltyPrograms/nearby`
- **Method**: `GET`
- **Auth Required**: Yes
- **Query Parameters**:
  - `latitude`: User's latitude
  - `longitude`: User's longitude
  - `radiusKm` (optional): Search radius in kilometers (default: 10)
  - `page` (optional): Page number (default: 1)
  - `pageSize` (optional): Items per page (default: 20)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: Similar to get all loyalty programs, but filtered by location

#### Search loyalty programs

- **URL**: `/api/loyaltyPrograms/search`
- **Method**: `GET`
- **Auth Required**: Yes
- **Query Parameters**:
  - `query`: Search term
  - `page` (optional): Page number (default: 1)
  - `pageSize` (optional): Items per page (default: 20)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: Similar to get all loyalty programs, but filtered by search term

### Rewards

#### Get rewards for a loyalty program

- **URL**: `/api/loyaltyPrograms/{id}/rewards`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty program ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "reward-uuid",
        "name": "Free Coffee",
        "description": "Get a free coffee of your choice",
        "loyaltyProgramId": "program-uuid",
        "pointsCost": 50,
        "isActive": true,
        "createdAt": "2023-03-14T12:00:00Z",
        "expiresAt": null
      }
    ]
    ```

#### Get specific reward for a loyalty program

- **URL**: `/api/loyaltyPrograms/{id}/rewards/{rewardId}`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty program ID
  - `rewardId`: The reward ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "id": "reward-uuid",
      "name": "Free Coffee",
      "description": "Get a free coffee of your choice",
      "loyaltyProgramId": "program-uuid",
      "pointsCost": 50,
      "isActive": true,
      "createdAt": "2023-03-14T12:00:00Z",
      "expiresAt": null
    }
    ```

#### Redeem a reward

- **URL**: `/api/rewards/redeem`
- **Method**: `POST`
- **Auth Required**: Yes
- **Request Body**:
  ```json
  {
    "loyaltyCardId": "card-uuid",
    "rewardId": "reward-uuid"
  }
  ```
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "success": true,
      "message": "Reward successfully redeemed",
      "redemptionCode": "ABC123XYZ",
      "reward": {
        "id": "reward-uuid",
        "name": "Free Coffee",
        "description": "Get a free coffee of your choice",
        "loyaltyProgramId": "program-uuid",
        "pointsCost": 50,
        "isActive": true,
        "createdAt": "2023-03-14T12:00:00Z",
        "expiresAt": null
      }
    }
    ```

### Loyalty Cards

#### Enroll in a loyalty program

- **URL**: `/api/loyaltyCards`
- **Method**: `POST`
- **Auth Required**: Yes
- **Request Body**:
  ```json
  {
    "loyaltyProgramId": "program-uuid"
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**:
    ```json
    {
      "id": "card-uuid",
      "customerId": "customer-uuid",
      "loyaltyProgramId": "program-uuid",
      "cardNumber": "123456789",
      "currentPoints": 0,
      "createdAt": "2023-03-14T12:00:00Z",
      "lastUsed": null,
      "status": "Active"
    }
    ```

#### Get customer's loyalty cards

- **URL**: `/api/loyaltyCards/mine`
- **Method**: `GET`
- **Auth Required**: Yes
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "card-uuid",
        "customerId": "customer-uuid",
        "loyaltyProgramId": "program-uuid",
        "cardNumber": "123456789",
        "currentPoints": 50,
        "createdAt": "2023-03-14T12:00:00Z",
        "lastUsed": "2023-03-14T14:00:00Z",
        "status": "Active"
      }
    ]
    ```

#### Get loyalty card by ID

- **URL**: `/api/loyaltyCards/{id}`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty card ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "id": "card-uuid",
      "customerId": "customer-uuid",
      "loyaltyProgramId": "program-uuid",
      "cardNumber": "123456789",
      "currentPoints": 50,
      "createdAt": "2023-03-14T12:00:00Z",
      "lastUsed": "2023-03-14T14:00:00Z",
      "status": "Active"
    }
    ```

#### Get transactions for a loyalty card

- **URL**: `/api/loyaltyCards/{id}/transactions`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty card ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "transaction-uuid",
        "loyaltyCardId": "card-uuid",
        "amount": 10.50,
        "pointsEarned": 10,
        "description": "Coffee purchase",
        "timestamp": "2023-03-14T14:00:00Z"
      }
    ]
    ```

#### Get QR code for a loyalty card

- **URL**: `/api/loyaltyCards/{id}/qr-code`
- **Method**: `GET`
- **Auth Required**: Yes
- **URL Parameters**:
  - `id`: The loyalty card ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "qrCode": "base64-encoded-qr-code-image"
    }
    ```

#### Get nearby stores

- **URL**: `/api/loyaltyCards/nearby-stores`
- **Method**: `GET`
- **Auth Required**: Yes
- **Query Parameters**:
  - `latitude`: User's latitude
  - `longitude`: User's longitude
  - `radiusKm` (optional): Search radius in kilometers (default: 10)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "store_1",
        "name": "Store 1",
        "distance": 0.5
      },
      {
        "id": "store_2",
        "name": "Store 2",
        "distance": 1.2
      }
    ]
    ```

### Transactions

#### Record a transaction

- **URL**: `/api/transactions`
- **Method**: `POST`
- **Auth Required**: Yes
- **Request Body**:
  ```json
  {
    "loyaltyCardId": "card-uuid",
    "amount": 10.50,
    "description": "Coffee purchase"
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**:
    ```json
    {
      "id": "transaction-uuid",
      "loyaltyCardId": "card-uuid",
      "amount": 10.50,
      "pointsEarned": 10,
      "description": "Coffee purchase",
      "timestamp": "2023-03-14T14:00:00Z"
    }
    ```

## Admin API

### Business Management

#### Get all businesses

- **URL**: `/api/admin/businesses`
- **Method**: `GET`
- **Auth Required**: Yes (Admin)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    [
      {
        "id": "business-uuid",
        "name": "Coffee Shop Chain",
        "createdAt": "2023-03-14T12:00:00Z"
      }
    ]
    ```

#### Create a new business

- **URL**: `/api/admin/businesses`
- **Method**: `POST`
- **Auth Required**: Yes (Admin)
- **Request Body**:
  ```json
  {
    "name": "Coffee Shop Chain",
    "address": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA"
    },
    "contactEmail": "info@coffeeshop.com",
    "phone": "+1234567890"
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**: Created business object

### Loyalty Program Management

#### Get all loyalty programs

- **URL**: `/api/admin/loyalty-programs`
- **Method**: `GET`
- **Auth Required**: Yes (Admin)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: List of all loyalty programs

#### Create a new loyalty program

- **URL**: `/api/admin/loyalty-programs`
- **Method**: `POST`
- **Auth Required**: Yes (Admin)
- **Request Body**:
  ```json
  {
    "name": "Coffee Rewards",
    "description": "Earn points with every coffee purchase",
    "businessId": "business-uuid",
    "brandId": "brand-uuid",
    "isActive": true
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**: Created loyalty program object

#### Get, Update, or Delete a loyalty program

- **URL**: `/api/admin/loyalty-programs/{id}`
- **Methods**: `GET`, `PUT`, `DELETE`
- **Auth Required**: Yes (Admin)
- **URL Parameters**:
  - `id`: The loyalty program ID
- **Success Responses**:
  - `GET`: 200 OK with program details
  - `PUT`: 200 OK with updated program details
  - `DELETE`: 204 No Content

### Reward Management

#### Create a new reward

- **URL**: `/api/admin/rewards`
- **Method**: `POST`
- **Auth Required**: Yes (Admin)
- **Request Body**:
  ```json
  {
    "name": "Free Coffee",
    "description": "Get a free coffee of your choice",
    "loyaltyProgramId": "program-uuid",
    "pointsCost": 50,
    "isActive": true,
    "expiresAt": null
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**: Created reward object

#### Update or Delete a reward

- **URL**: `/api/admin/rewards/{id}`
- **Methods**: `PUT`, `DELETE`
- **Auth Required**: Yes (Admin)
- **URL Parameters**:
  - `id`: The reward ID
- **Success Responses**:
  - `PUT`: 200 OK with updated reward details
  - `DELETE`: 204 No Content

### Analytics

#### Get dashboard analytics

- **URL**: `/api/admin/analytics/dashboard`
- **Method**: `GET`
- **Auth Required**: Yes (Admin)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "totalCustomers": 1250,
      "activePrograms": 5,
      "totalTransactions": 5680,
      "recentRedemptions": 124
    }
    ```

#### Get customer analytics

- **URL**: `/api/admin/analytics/customers`
- **Method**: `GET`
- **Auth Required**: Yes (Admin)
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: Customer analytics data

#### Get transaction analytics

- **URL**: `/api/admin/analytics/transactions`
- **Method**: `GET`
- **Auth Required**: Yes (Admin)
- **Query Parameters**:
  - `startDate` (optional): Start date for analytics
  - `endDate` (optional): End date for analytics
  - `programId` (optional): Filter by program ID
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: Transaction analytics data

## Staff API

### Customer Management

#### Lookup a customer

- **URL**: `/api/staff/customers/lookup`
- **Method**: `GET`
- **Auth Required**: Yes (Staff)
- **Query Parameters**:
  - `query`: Card number, email, or phone number
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "customer": {
        "id": "customer-uuid",
        "email": "customer@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "createdAt": "2023-03-14T12:00:00Z",
        "updatedAt": "2023-03-14T12:00:00Z"
      },
      "loyaltyCards": [
        {
          "id": "card-uuid",
          "customerId": "customer-uuid",
          "loyaltyProgramId": "program-uuid",
          "cardNumber": "123456789",
          "currentPoints": 50,
          "createdAt": "2023-03-14T12:00:00Z",
          "lastUsed": "2023-03-14T14:00:00Z",
          "status": "Active"
        }
      ]
    }
    ```

### Transaction Processing

#### Record a transaction

- **URL**: `/api/staff/transactions`
- **Method**: `POST`
- **Auth Required**: Yes (Staff)
- **Request Body**:
  ```json
  {
    "loyaltyCardId": "card-uuid",
    "amount": 10.50,
    "description": "In-store coffee purchase"
  }
  ```
- **Success Response**: 
  - **Code**: 201 Created
  - **Content**: Created transaction object

### Reward Redemption

#### Redeem a reward

- **URL**: `/api/staff/rewards/redeem`
- **Method**: `POST`
- **Auth Required**: Yes (Staff)
- **Request Body**:
  ```json
  {
    "loyaltyCardId": "card-uuid",
    "rewardId": "reward-uuid"
  }
  ```
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**: Similar to customer reward redemption

#### Validate a redemption code

- **URL**: `/api/staff/rewards/validate/{code}`
- **Method**: `GET`
- **Auth Required**: Yes (Staff)
- **URL Parameters**:
  - `code`: The redemption code
- **Success Response**: 
  - **Code**: 200 OK
  - **Content**:
    ```json
    {
      "valid": true,
      "reward": {
        "id": "reward-uuid",
        "name": "Free Coffee",
        "description": "Get a free coffee of your choice",
        "loyaltyProgramId": "program-uuid",
        "pointsCost": 50,
        "isActive": true,
        "createdAt": "2023-03-14T12:00:00Z",
        "expiresAt": null
      },
      "customer": {
        "id": "customer-uuid",
        "email": "customer@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "createdAt": "2023-03-14T12:00:00Z",
        "updatedAt": "2023-03-14T12:00:00Z"
      }
    }
    ``` 