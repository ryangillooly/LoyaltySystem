# Loyalty System API Diagrams

## High-Level Architecture

```mermaid
flowchart TB
    subgraph Users
        C[Customer User]
        S[Staff User]
        A[Admin User]
    end

    subgraph "Loyalty System API"
        CApi[Customer API]
        SApi[Staff API]
        AApi[Admin API]
        DB[(Database)]
    end
    
    C -->|Uses| CApi
    S -->|Uses| SApi
    A -->|Uses| AApi
    
    CApi -->|Reads/Writes| DB
    SApi -->|Reads/Writes| DB
    AApi -->|Reads/Writes| DB
    
    style CApi fill:#90EE90
    style SApi fill:#ADD8E6
    style AApi fill:#FFB6C1
```

## Customer API Endpoints

```mermaid
classDiagram
    class CustomerAuthentication {
        POST /api/auth/register
        POST /api/auth/login
        GET /api/auth/loyalty-summary
    }
    
    class LoyaltyPrograms {
        GET /api/loyaltyPrograms
        GET /api/loyaltyPrograms/{id}
        GET /api/loyaltyPrograms/brand/{brandId}
        GET /api/loyaltyPrograms/nearby
        GET /api/loyaltyPrograms/search
    }
    
    class LoyaltyCards {
        POST /api/loyaltyCards
        GET /api/loyaltyCards/mine
        GET /api/loyaltyCards/{id}
        GET /api/loyaltyCards/{id}/transactions
        GET /api/loyaltyCards/{id}/qr-code
        GET /api/loyaltyCards/nearby-stores
    }
    
    class Rewards {
        GET /api/loyaltyPrograms/{id}/rewards
        GET /api/loyaltyPrograms/{id}/rewards/{rewardId}
        POST /api/rewards/redeem
    }
    
    class Transactions {
        POST /api/transactions
    }
    
    CustomerAuthentication -- LoyaltyPrograms : authenticated user accesses
    CustomerAuthentication -- LoyaltyCards : authenticated user accesses
    CustomerAuthentication -- Rewards : authenticated user accesses
    CustomerAuthentication -- Transactions : authenticated user accesses
    LoyaltyPrograms -- Rewards : program contains rewards
    LoyaltyCards -- Transactions : card has transactions
```

## Admin API Endpoints

```mermaid
classDiagram
    class AdminAuthentication {
        POST /api/admin/auth/login
    }
    
    class BusinessManagement {
        GET /api/admin/businesses
        POST /api/admin/businesses
    }
    
    class LoyaltyProgramManagement {
        GET /api/admin/loyalty-programs
        POST /api/admin/loyalty-programs
        GET /api/admin/loyalty-programs/{id}
        PUT /api/admin/loyalty-programs/{id}
        DELETE /api/admin/loyalty-programs/{id}
    }
    
    class RewardManagement {
        POST /api/admin/rewards
        PUT /api/admin/rewards/{id}
        DELETE /api/admin/rewards/{id}
    }
    
    class Analytics {
        GET /api/admin/analytics/dashboard
        GET /api/admin/analytics/customers
        GET /api/admin/analytics/transactions
    }
    
    AdminAuthentication -- BusinessManagement : authenticated admin accesses
    AdminAuthentication -- LoyaltyProgramManagement : authenticated admin accesses
    AdminAuthentication -- RewardManagement : authenticated admin accesses
    AdminAuthentication -- Analytics : authenticated admin accesses
    BusinessManagement -- LoyaltyProgramManagement : business contains programs
    LoyaltyProgramManagement -- RewardManagement : program contains rewards
```

## Staff API Endpoints

```mermaid
classDiagram
    class StaffAuthentication {
        POST /api/staff/auth/login
    }
    
    class CustomerManagement {
        GET /api/staff/customers/lookup
    }
    
    class TransactionProcessing {
        POST /api/staff/transactions
    }
    
    class RewardRedemption {
        POST /api/staff/rewards/redeem
        GET /api/staff/rewards/validate/{code}
    }
    
    StaffAuthentication -- CustomerManagement : authenticated staff accesses
    StaffAuthentication -- TransactionProcessing : authenticated staff accesses
    StaffAuthentication -- RewardRedemption : authenticated staff accesses
    CustomerManagement -- TransactionProcessing : for customer
    CustomerManagement -- RewardRedemption : for customer
```

## Data Models and Relationships

```mermaid
erDiagram
    Business ||--o{ LoyaltyProgram : "has many"
    LoyaltyProgram ||--o{ Reward : "offers"
    LoyaltyProgram ||--o{ LoyaltyCard : "has subscribers"
    Customer ||--o{ LoyaltyCard : "holds"
    LoyaltyCard ||--o{ Transaction : "records"
    LoyaltyCard ||--o{ Redemption : "redeems"
    Reward ||--o{ Redemption : "is part of"
    
    Business {
        uuid id
        string name
        string contactEmail
        string phone
    }
    
    LoyaltyProgram {
        uuid id
        string name
        string description
        uuid businessId
        uuid brandId
        boolean isActive
    }
    
    Reward {
        uuid id
        string name
        string description
        uuid loyaltyProgramId
        int pointsCost
        boolean isActive
        datetime expiresAt
    }
    
    Customer {
        uuid id
        string email
        string firstName
        string lastName
    }
    
    LoyaltyCard {
        uuid id
        uuid customerId
        uuid loyaltyProgramId
        string cardNumber
        int currentPoints
        string status
    }
    
    Transaction {
        uuid id
        uuid loyaltyCardId
        double amount
        int pointsEarned
        string description
        datetime timestamp
    }
    
    Redemption {
        uuid id
        uuid loyaltyCardId
        uuid rewardId
        string redemptionCode
        boolean success
        datetime timestamp
    }
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant AuthService
    participant Database
    
    User->>API: POST /api/auth/register
    API->>AuthService: Register user
    AuthService->>Database: Create user
    Database-->>AuthService: User created
    AuthService-->>API: Return user details
    API-->>User: 201 Created (User details)
    
    User->>API: POST /api/auth/login
    API->>AuthService: Validate credentials
    AuthService->>Database: Find user
    Database-->>AuthService: User found
    AuthService->>AuthService: Generate JWT token
    AuthService-->>API: Return token and user
    API-->>User: 200 OK (Token + user details)
```

## Transaction Flow

```mermaid
sequenceDiagram
    participant Customer
    participant Staff
    participant API
    participant TransactionService
    participant LoyaltyCardService
    participant Database
    
    alt Customer records transaction
        Customer->>API: POST /api/transactions
    else Staff records transaction
        Staff->>API: POST /api/staff/transactions
    end
    
    API->>TransactionService: Create transaction
    TransactionService->>LoyaltyCardService: Get card details
    LoyaltyCardService->>Database: Query card
    Database-->>LoyaltyCardService: Card details
    LoyaltyCardService-->>TransactionService: Card found
    
    TransactionService->>TransactionService: Calculate points
    TransactionService->>Database: Save transaction
    TransactionService->>LoyaltyCardService: Update points balance
    LoyaltyCardService->>Database: Update card
    
    Database-->>API: Transaction completed
    API-->>Customer: 201 Created (Transaction details)
```

## Reward Redemption Flow

```mermaid
sequenceDiagram
    participant Customer
    participant Staff
    participant API
    participant RewardService
    participant LoyaltyCardService
    participant Database
    
    alt Customer redeems reward
        Customer->>API: POST /api/rewards/redeem
    else Staff redeems reward
        Staff->>API: POST /api/staff/rewards/redeem
    end
    
    API->>RewardService: Redeem reward
    RewardService->>LoyaltyCardService: Get card points
    LoyaltyCardService->>Database: Query card
    Database-->>LoyaltyCardService: Card details
    LoyaltyCardService-->>RewardService: Points balance
    
    RewardService->>Database: Query reward
    Database-->>RewardService: Reward details
    
    RewardService->>RewardService: Verify sufficient points
    RewardService->>RewardService: Generate redemption code
    RewardService->>Database: Save redemption
    RewardService->>LoyaltyCardService: Deduct points
    LoyaltyCardService->>Database: Update card
    
    Database-->>API: Redemption completed
    API-->>Customer: 200 OK (Redemption details with code)
``` 