# Loyalty System Database Schema Description

## Core Entities

### Businesses
Represents top-level business entities that own multiple brands.
- **Primary Key**: Id (GUID)
- **Relationships**:
  - One-to-many with Brands
  - One-to-one with BusinessContacts and BusinessAddresses

### Brands
Represents companies or product lines that belong to a business and offer loyalty programs.
- **Primary Key**: Id (GUID)
- **Foreign Keys**: BusinessId (references Businesses)
- **Relationships**:
  - Many-to-one with Businesses
  - One-to-many with Stores
  - One-to-many with LoyaltyPrograms
  - One-to-one with BrandContacts and BrandAddresses

### Stores
Physical or online locations belonging to a brand.
- **Primary Key**: Id (GUID)
- **Foreign Keys**: BrandId (references Brands)
- **Relationships**:
  - Many-to-one with Brands
  - One-to-one with StoreAddresses and StoreGeoLocations
  - One-to-many with StoreOperatingHours
  - One-to-many with Transactions

### Customers
End users who participate in loyalty programs.
- **Primary Key**: Id (GUID)
- **Relationships**:
  - One-to-many with LoyaltyCards

### LoyaltyPrograms
Rules and configurations for loyalty schemes offered by brands.
- **Primary Key**: Id (GUID)
- **Foreign Keys**: BrandId (references Brands)
- **Relationships**:
  - Many-to-one with Brands
  - One-to-one with ProgramExpirationPolicies
  - One-to-many with Rewards
  - One-to-many with LoyaltyCards
- **Type Values**:
  - 1 = Stamp-based
  - 2 = Points-based

### LoyaltyCards
Customer's membership in a loyalty program.
- **Primary Key**: Id (GUID)
- **Foreign Keys**: 
  - ProgramId (references LoyaltyPrograms)
  - CustomerId (references Customers)
- **Relationships**:
  - Many-to-one with Customers
  - Many-to-one with LoyaltyPrograms
  - One-to-many with Transactions
  - One-to-many with CardLinks
- **Status Values**:
  - 1 = Active
  - 2 = Expired
  - 3 = Suspended

### Rewards
Incentives offered to customers within loyalty programs.
- **Primary Key**: Id (GUID)
- **Foreign Keys**: ProgramId (references LoyaltyPrograms)
- **Relationships**:
  - Many-to-one with LoyaltyPrograms
  - One-to-many with Transactions (when redeemed)

### Transactions
Records of loyalty activity (stamp issuance, point accrual, redemptions).
- **Primary Key**: Id (GUID)
- **Foreign Keys**:
  - CardId (references LoyaltyCards)
  - RewardId (references Rewards, nullable)
  - StoreId (references Stores)
- **Relationships**:
  - Many-to-one with LoyaltyCards
  - Many-to-one with Rewards (optional)
  - Many-to-one with Stores
- **Type Values**:
  - 1 = StampIssuance
  - 2 = PointsIssuance
  - 3 = RewardRedemption
  - 4 = StampVoid
  - 5 = PointsVoid

## Supporting Entities

### BusinessContacts
Contact information for businesses.
- **Primary Key**: BusinessId (GUID)
- **Foreign Keys**: BusinessId (references Businesses)
- **Relationships**: One-to-one with Businesses

### BusinessAddresses
Physical address for business headquarters.
- **Primary Key**: BusinessId (GUID)
- **Foreign Keys**: BusinessId (references Businesses)
- **Relationships**: One-to-one with Businesses

### BrandContacts
Contact information for brands.
- **Primary Key**: BrandId (GUID)
- **Foreign Keys**: BrandId (references Brands)
- **Relationships**: One-to-one with Brands

### BrandAddresses
Physical address for brand headquarters.
- **Primary Key**: BrandId (GUID)
- **Foreign Keys**: BrandId (references Brands)
- **Relationships**: One-to-one with Brands

### StoreAddresses
Physical address for store locations.
- **Primary Key**: StoreId (GUID)
- **Foreign Keys**: StoreId (references Stores)
- **Relationships**: One-to-one with Stores

### StoreGeoLocations
GPS coordinates for store locations.
- **Primary Key**: StoreId (GUID)
- **Foreign Keys**: StoreId (references Stores)
- **Relationships**: One-to-one with Stores

### StoreOperatingHours
Business hours for store locations.
- **Primary Key**: Composite (StoreId, DayOfWeek)
- **Foreign Keys**: StoreId (references Stores)
- **Relationships**: Many-to-one with Stores

### ProgramExpirationPolicies
Rules for when loyalty cards expire.
- **Primary Key**: ProgramId (GUID)
- **Foreign Keys**: ProgramId (references LoyaltyPrograms)
- **Relationships**: One-to-one with LoyaltyPrograms
- **ExpirationType Values**:
  - 1 = Days
  - 2 = Months
  - 3 = Years

### CardLinks
Associations between loyalty cards and external identifiers (payment cards, etc.).
- **Primary Key**: Id (GUID)
- **Foreign Keys**: CardId (references LoyaltyCards)
- **Relationships**: Many-to-one with LoyaltyCards
- **LinkType Values**:
  - 1 = PaymentCard
  - 2 = PhoneNumber
  - 3 = Email
  - 4 = Other

## Key Design Features

1. **Multi-level Business Hierarchy**: The system supports a two-level hierarchy with Businesses at the top level and Brands as their children, enabling multi-brand management under a single business entity.

2. **Value Object Pattern**: Address information is stored in separate tables (BusinessAddresses, BrandAddresses, StoreAddresses) to encapsulate this complex value object.

3. **Flexible Loyalty Rules**: The LoyaltyPrograms table supports both stamp-based and points-based programs with configurable thresholds, conversion rates, and limits.

4. **Time-Bound Rewards**: Rewards can have validity periods (ValidFrom, ValidTo) to support seasonal or limited-time offers.

5. **Rich Transaction Data**: The Transactions table records all loyalty activity with detailed metadata about store location, staff member, and POS integration.

6. **External Integration**: The CardLinks table enables linking loyalty cards to external identifiers for POS integration.

7. **Schema Evolution Support**: Most tables include CreatedAt and UpdatedAt timestamps for auditing and data lifecycle management. 