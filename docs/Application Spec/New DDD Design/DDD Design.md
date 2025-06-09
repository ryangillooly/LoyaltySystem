# Loyalty System – Comprehensive Business Requirements & Domain‑Driven Design (DDD) Specification  
*Version 0.5 — Domain Models Enhanced*

---

## 0. Table of Contents
1. High‑Level Business Goals  
2. Non‑Functional Requirements (NFRs)  
3. Subdomain & Bounded‑Context Map  
4. Context‑by‑Context Specifications  
    4.1 Loyalty Program Context (Core)  
    4.2 Customer Identity & Consent Context  
    4.3 Staff + Access‑Control Context  
    4.4 Business / Brand / Store Management Context  
    4.5 Integration Context (POS, Webhooks, Public API)  
    4.6 Notification Context  
    4.7 Audit & Analytics Context  
5. Cross‑Cutting Concerns  
6. Domain Events Catalogue  
7. Ubiquitous Language Glossary  
8. Appendix A – Merchant Integration Options  

---

## 1. High‑Level Business Goals ✅
- Attract and retain customers for client businesses (brands, stores, franchises).  
- Enable businesses to create and manage loyalty programs (points, tiers, rewards).  
- Allow customers to enroll, earn, and redeem rewards easily.  
- Provide staff/admin tools for managing users, customers, rewards, and analytics.  
- Support multi‑tenancy (multiple brands/businesses/stores under one platform).  
- Deliver actionable analytics to businesses about customer engagement and program performance.  
- Ensure security, privacy, and compliance (e.g., GDPR).  

---

## 2. Non‑Functional Requirements (NFRs) 🔒

| Category | Requirement |
|----------|-------------|
| **Performance** | p95 < 200 ms for write APIs, p99 < 300 ms for read APIs at 1 k rps per tenant |
| **Scalability** | Horizontally scale to 10 k rps aggregate traffic (peak Black‑Friday) |
| **Availability** | 99.9 % monthly per public SLA |
| **Security** | GDPR, PCI‑DSS (SAQ‑A) scope, OWASP Top‑10 mitigations |
| **Observability** | Structured logging, trace IDs propagated across contexts, SLO dashboards |
| **Cost** | Baseline infra < £300 / month for 100 SMB tenants |

---

## 3. Subdomain & Bounded‑Context Map 📐

| Context | Type | Purpose | Interfaces Out |
|---------|------|---------|----------------|
| **Loyalty Program** | *Core* | Earn/redeem logic, stamps/points, rewards | Domain events |
| **Customer Identity & Consent** | Supporting | Profiles, comms prefs, GDPR | Events, REST |
| **Staff & Access Control** | Supporting | RBAC, authN/Z, MFA | REST |
| **Business Management** | Supporting | Hierarchy, subscription, billing | Events, REST |
| **Integration** | Supporting | POS ingestion, Public API, Webhooks | EventBridge, REST |
| **Notification** | Generic | Email/SMS/Push delivery | SMTP, Twilio |
| **Audit & Analytics** | Supporting | Immutable logs, BI exports | S3, Redshift |

> **Anti‑Corruption Layers (ACL)** protect each context from external models (e.g., POS line‑items vs. internal `Transaction`).

---

## 4. Context‑by‑Context Specifications

### 4.1 Loyalty Program Context (Core) ☕

#### 4.1.1 Aggregates & Entities

##### LoyaltyProgram (Aggregate Root)

```csharp
class LoyaltyProgram
{
    public Guid Id { get; private set; }
    public Guid BrandId { get; private set; }
    public string Name { get; private set; }
    public ProgramType Type { get; private set; }
    public ConversionRate? Rate { get; private set; }
    public DailyLimits Limits { get; private set; }
    public ExpirationPolicy Expiry { get; private set; }
    public IList<Reward> Rewards { get; private set; }
    public FraudPolicy? PolicyOverride { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new DomainException("Program name is required");
            
        if (Type == ProgramType.Points && Rate == null)
            throw new DomainException("Points program must have conversion rate");
            
        if (Type == ProgramType.Stamp && Rate != null)
            throw new DomainException("Stamp program cannot have conversion rate");
            
        if (!Rewards.Any(r => r.IsActive))
            throw new DomainException("Program must have at least one active reward");
    }
}

public enum ProgramType 
{
    Stamp,
    Points
}
```

###### Key Value‑Objects

```csharp
public record ConversionRate
{
    public decimal Multiplier { get; init; }

    public ConversionRate(decimal multiplier)
    {
        if (multiplier <= 0)
            throw new DomainException("Multiplier must be positive");
        Multiplier = multiplier;
    }
}

public record DailyLimits
{
    public int? StampPerDay { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }

    public DailyLimits(int? stampPerDay, decimal? minAmount)
    {
        if (stampPerDay.HasValue && stampPerDay <= 0)
            throw new DomainException("Daily stamp limit must be positive");
        if (minAmount.HasValue && minAmount <= 0)
            throw new DomainException("Minimum transaction amount must be positive");
            
        StampPerDay = stampPerDay;
        MinimumTransactionAmount = minAmount;
    }
}

public record ExpirationPolicy
{
    public TimeSpan RollingWindow { get; init; }
    public bool FIFO { get; init; }

    public ExpirationPolicy(TimeSpan rollingWindow, bool fifo)
    {
        if (rollingWindow <= TimeSpan.Zero)
            throw new DomainException("Rolling window must be positive");
        RollingWindow = rollingWindow;
        FIFO = fifo;
    }
}

public record FraudPolicy
{
    public TimeSpan? CooldownBetweenScans { get; init; }
    public int? MaxRedemptionsPerDay { get; init; }
    public int? MaxPointsPerVelocityWindow { get; init; }
    public TimeSpan? VelocityWindow { get; init; }
    public int? MaxDistanceMeters { get; init; }
    public bool BlockOutsideOperatingHours { get; init; }

    public FraudPolicy(
        TimeSpan? cooldown = null,
        int? maxRedemptions = null,
        int? maxPoints = null,
        TimeSpan? velocityWindow = null,
        int? maxDistance = null,
        bool blockOutsideHours = false)
    {
        if (cooldown.HasValue && cooldown <= TimeSpan.Zero)
            throw new DomainException("Cooldown must be positive");
        if (maxRedemptions.HasValue && maxRedemptions <= 0)
            throw new DomainException("Max redemptions must be positive");
        if (maxPoints.HasValue && maxPoints <= 0)
            throw new DomainException("Max points must be positive");
        if (velocityWindow.HasValue && velocityWindow <= TimeSpan.Zero)
            throw new DomainException("Velocity window must be positive");
        if (maxDistance.HasValue && maxDistance <= 0)
            throw new DomainException("Max distance must be positive");

        CooldownBetweenScans = cooldown;
        MaxRedemptionsPerDay = maxRedemptions;
        MaxPointsPerVelocityWindow = maxPoints;
        VelocityWindow = velocityWindow;
        MaxDistanceMeters = maxDistance;
        BlockOutsideOperatingHours = blockOutsideHours;
    }
}
```

##### Reward (Entity)

```csharp
public class Reward
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public RewardType Type { get; private set; }
    public int RequiredValue { get; private set; }
    public int? MaxRedemptionsPerDay { get; private set; }
    public TimeSpan? CooldownMinutes { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Title))
            throw new DomainException("Reward title is required");
        if (RequiredValue <= 0)
            throw new DomainException("Required value must be positive");
        if (MaxRedemptionsPerDay.HasValue && MaxRedemptionsPerDay <= 0)
            throw new DomainException("Max redemptions must be positive");
        if (CooldownMinutes.HasValue && CooldownMinutes <= TimeSpan.Zero)
            throw new DomainException("Cooldown must be positive");
        if (ValidFrom.HasValue && ValidTo.HasValue && ValidFrom >= ValidTo)
            throw new DomainException("Valid from must be before valid to");
    }
}

public enum RewardType
{
    FreeItem,
    PercentageOff,
    MonetaryValue
}
```

##### LoyaltyCard (Aggregate Root)

```csharp
public class LoyaltyCard
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid CustomerId { get; private set; }
    public CardStatus Status { get; private set; }
    public int StampsCollected { get; private set; }
    public decimal PointsBalance { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public IList<Transaction> Transactions { get; private set; }

    public void AddStamps(int amount, FraudPolicy policy)
    {
        ValidateActive();
        ValidateNotExpired();
        ValidateFraudPolicy(policy);
        
        StampsCollected += amount;
        LastActivityAt = DateTime.UtcNow;
    }

    public void AddPoints(decimal amount, FraudPolicy policy)
    {
        ValidateActive();
        ValidateNotExpired();
        ValidateFraudPolicy(policy);
        
        PointsBalance += amount;
        LastActivityAt = DateTime.UtcNow;
    }

    public void RedeemReward(Reward reward, FraudPolicy policy)
    {
        ValidateActive();
        ValidateNotExpired();
        ValidateFraudPolicy(policy);
        ValidateRewardEligibility(reward);
        
        if (reward.Type == RewardType.FreeItem)
            StampsCollected -= reward.RequiredValue;
        else
            PointsBalance -= reward.RequiredValue;
            
        LastActivityAt = DateTime.UtcNow;
    }

    private void ValidateActive()
    {
        if (Status != CardStatus.Active)
            throw new DomainException("Card must be active");
    }

    private void ValidateNotExpired()
    {
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
            throw new DomainException("Card has expired");
    }

    private void ValidateFraudPolicy(FraudPolicy policy)
    {
        // Implement fraud policy validation logic
    }

    private void ValidateRewardEligibility(Reward reward)
    {
        if (reward.Type == RewardType.FreeItem && StampsCollected < reward.RequiredValue)
            throw new DomainException("Insufficient stamps for reward");
        if (reward.Type != RewardType.FreeItem && PointsBalance < reward.RequiredValue)
            throw new DomainException("Insufficient points for reward");
    }
}

public enum CardStatus
{
    Active,
    Suspended,
    Expired
}
```

##### Transaction (Entity)

```csharp
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid? RewardId { get; private set; }
    public int? StampAmount { get; private set; }
    public decimal? PointsAmount { get; private set; }
    public decimal? TransactionAmount { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid? StaffId { get; private set; }
    public string? PosTransactionId { get; private set; }
    public TransactionSource Source { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Validate()
    {
        if (Type == TransactionType.StampIssuance && !StampAmount.HasValue)
            throw new DomainException("Stamp amount required for stamp issuance");
        if (Type == TransactionType.PointsIssuance && !PointsAmount.HasValue)
            throw new DomainException("Points amount required for points issuance");
        if (Type == TransactionType.RewardRedemption && !RewardId.HasValue)
            throw new DomainException("Reward ID required for redemption");
    }
}

public enum TransactionType
{
    StampIssuance,
    PointsIssuance,
    RewardRedemption,
    StampVoid,
    PointsVoid
}
```

#### 4.1.2 Domain Services
- `LoyaltyTransactionService`
- `RewardRedemptionService`
- **FraudEvaluationService** – invoked before issue/redemption:
  - Evaluates effective `FraudPolicy` (Store → Program → Brand default).
  - Emits `FraudAttemptDetected` event if blocked.
- `LoyaltyCardService` - Customer enrollment and card lifecycle management
- `CardLinkService` - Managing payment method linking to loyalty cards
- `AuditService` - Recording system changes and security events

#### 4.1.3 Key Workflows

##### Stamp Issuance Workflow
1. Staff identifies customer via QR code scan or lookup
2. System retrieves customer's loyalty card
3. System validates card status and program rules
4. System checks daily stamp limit compliance
5. If valid, stamps are issued and recorded as a transaction
6. Customer and staff receive confirmation
7. `StampsIssued` event is published for notifications/analytics

##### Points Addition Workflow (POS Integration)
1. Customer makes purchase and provides loyalty identifier (QR/card/phone)
2. POS system sends transaction data to loyalty API
3. System identifies customer's loyalty card
4. System validates transaction amount against program rules
5. System calculates points based on conversion rate
6. Points are added to card and recorded as a transaction
7. `PointsAdded` event is published for notifications/analytics
8. Customer receives confirmation (receipt, notification, etc.)

##### Reward Redemption Workflow
1. Staff identifies customer via QR code scan or lookup
2. System retrieves customer's available rewards based on loyalty balance
3. Staff selects reward to redeem with customer
4. System validates reward eligibility (balance, validity period)
5. System processes redemption, deducts stamps/points, records transaction
6. `RewardRedeemed` event is published for notifications/analytics
7. Customer and staff receive confirmation

#### 4.1.4 Business Rules & Invariants

##### Card Type Consistency
- A card's type (Stamp/Points) must match its program's type
- Stamp-based cards track StampsCollected
- Points-based cards track PointsBalance

##### Card Status Rules
- Only Active cards can receive stamps/points or redeem rewards
- Expired or Suspended cards cannot perform transactions
- Cards have a status lifecycle: Active → Expired or Active → Suspended

##### Daily Stamp Limits
- If a program has a DailyStampLimit, cards cannot exceed this limit
- The limit is enforced across all stores for the same day

##### Points-based Transaction Rules
- Points are calculated based on the program's PointsConversionRate
- Transactions below MinimumTransactionAmount (if set) earn no points

##### Reward Redemption Rules
- Stamp-based: StampsCollected ≥ Reward.RequiredValue
- Points-based: PointsBalance ≥ Reward.RequiredValue
- Rewards can only be redeemed during their validity period
- A reward must be Active to be redeemable
- Rewards can only be redeemed against cards in the same program

##### Transaction Integrity Rules
- All transactions must have valid timestamps
- Transactions cannot be backdated to circumvent daily limits
- Transactions must occur at stores belonging to the program's brand
- Staff-initiated transactions must have a valid StaffID with appropriate permissions
- POS-linked transactions should include PosTransactionID for reconciliation

##### Multi-Currency Rules
- Each program must specify base currency
- Exchange rates updated daily
- Points calculations rounded per program rules
- Transactions processed in local currency
- Points calculated using daily exchange rate
- Minimum spend requirements adjusted by currency

##### API Rate Limiting Rules
- Basic: 1000 requests/hour
- Premium: 10000 requests/hour
- Enterprise: Custom limits
- Authentication: 10 requests/minute
- Analytics: 100 requests/hour
- Transaction endpoints: 500 requests/minute

#### 4.1.5 Repositories
| Interface | Purpose |
|-----------|---------|
| `ILoyaltyProgramRepository` | CRUD with optimistic‑locking |
| `ILoyaltyCardRepository` | `GetByCustomer`, `AddTransaction`, `GetRecentActivity` |

---

### 4.2 Customer Identity & Consent Context 👤

#### Customer (Aggregate Root)

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; }
    public CommsPreferences CommsPreferences { get; private set; }
    public ConsentSet ConsentSet { get; private set; }
    public string PreferredLanguage { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public DateTime? LastActivityDate { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; }
    public SocialLogins SocialLogins { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdatePersonalInfo(PersonalInfo info)
    {
        info.Validate();
        PersonalInfo = info;
        UpdatedAt = DateTime.UtcNow;
        // EmitEvent(new CustomerPersonalInfoUpdated(Id, info));
    }

    public void UpdateCommsPreferences(CommsPreferences prefs)
    {
        CommsPreferences = prefs;
        UpdatedAt = DateTime.UtcNow;
        // EmitEvent(new CustomerCommsPreferencesUpdated(Id, prefs));
    }

    public void UpdateConsent(ConsentSet consent)
    {
        if (!consent.AllRequiredGranted())
            throw new DomainException("All required consents must be granted");
        ConsentSet = consent;
        UpdatedAt = DateTime.UtcNow;
        // EmitEvent(new CustomerConsentUpdated(Id, consent));
    }

    public void Validate()
    {
        PersonalInfo.Validate();
        if (!ConsentSet.AllRequiredGranted())
            throw new DomainException("Required consents must be granted");
    }
}

public record PersonalInfo
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Address? Address { get; init; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(FirstName))
            throw new DomainException("First name is required");
        if (string.IsNullOrEmpty(LastName))
            throw new DomainException("Last name is required");
        if (string.IsNullOrEmpty(Email) || !IsValidEmail(Email))
            throw new DomainException("Valid email is required");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public record CommsPreferences
{
    public bool AllowEmail { get; init; }
    public bool AllowSMS { get; init; }
    public bool AllowPush { get; init; }
    public bool MarketingConsent { get; init; }
}

public record ConsentSet
{
    public bool GDPRConsent { get; init; }
    public bool DataProcessingConsent { get; init; }
    public bool MarketingConsent { get; init; }
    public DateTime ConsentDate { get; init; }

    public bool AllRequiredGranted() =>
        GDPRConsent && DataProcessingConsent;
}

public record SocialLogins
{
    public string? GoogleId { get; init; }
    public string? AppleId { get; init; }
    public bool EmailVerified { get; init; }
    public string? SocialLoginProvider { get; init; }
    public string? SocialLoginEmail { get; init; }
    public DateTime? LastSocialLoginAt { get; init; }
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Blocked
}
```

#### AuthToken (Entity)

```csharp
public class AuthToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public UserType UserType { get; private set; }
    public string Token { get; private set; }
    public TokenType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void MarkAsUsed()
    {
        if (UsedAt.HasValue)
            throw new DomainException("Token has already been used");
        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("Token has expired");
            
        UsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !UsedAt.HasValue && DateTime.UtcNow <= ExpiresAt;
    }
}

public enum UserType
{
    Customer,
    Staff
}

public enum TokenType
{
    EmailVerification,
    PasswordReset
}
```

##### Domain Services
- `CustomerRegistrationService`
- `GDPRExportService`

##### Domain Events
- `CustomerRegistered`
- `CustomerDeleted`
- `ConsentUpdated`

#### 4.2.1 Key Workflows

##### Email Verification Workflow
1. User signs up (Customer or Staff registration)
2. System generates secure verification token
3. System stores token with expiration
4. EmailService sends verification email with token link
5. User clicks verification link
6. System validates token (exists, not expired, not used)
7. System marks email as verified
8. System marks token as used
9. `EmailVerified` event is published
10. User is redirected to appropriate page

##### Password Reset Workflow
1. User requests password reset
2. System validates email exists
3. System generates secure reset token
4. System invalidates any existing reset tokens
5. EmailService sends reset email with token link
6. User clicks reset link
7. System validates token
8. User enters new password
9. System validates password meets requirements
10. System updates password and marks token as used
11. `PasswordReset` event is published
12. Confirmation email is sent

##### Social Authentication Flow
1. User initiates social login
2. System redirects to provider
3. Provider authenticates user
4. Provider returns with auth code
5. System validates auth code
6. System creates/updates user account
7. System issues JWT token

#### 4.2.2 Security Requirements

##### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character
- No common passwords
- No personal information
- Passwords must be hashed (using bcrypt)

##### Token Security
- Cryptographically secure random generation
- Minimum 32 bytes of entropy
- URL-safe encoding
- Exact string comparison
- Timing attack prevention
- Rate limiting on validation attempts

##### Email Security
- Required for all new accounts
- Required after email changes
- Grace period for unverified accounts
- Maximum verification attempts
- Rate limiting per email address
- Cooldown period between requests

##### Social Login Security
- CSRF token validation
- State parameter verification
- Nonce validation for Apple Sign In
- Secure token storage
- Rate limiting on auth endpoints
- IP-based blocking
- Fraud detection integration

##### Domain Services
- `CustomerRegistrationService`
- `GDPRExportService`
- `AuthenticationService` - Managing user authentication and tokens
- `EmailService` - Verification and notification emails
- `SocialAuthenticationService` - OAuth flows for social providers

##### Domain Events
- `CustomerRegistered`
- `CustomerDeleted`
- `ConsentUpdated`
- `EmailVerified`
- `PasswordReset`

---

### 4.3 Staff & Access‑Control Context 🛂

#### StaffUser (Aggregate Root)

```csharp
public class StaffUser
{
    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid? BrandId { get; private set; }
    public Guid? StoreId { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; }
    public HashSet<Role> Roles { get; private set; }
    public StaffStatus Status { get; private set; }
    public bool MfaEnabled { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void AssignRole(Role role)
    {
        ValidateRoleAssignment(role);
        Roles.Add(role);
        if (role.RequiresMfa && !MfaEnabled)
            throw new DomainException("MFA must be enabled for this role");
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Role role)
    {
        Roles.Remove(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableMfa()
    {
        MfaEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        if (Status != StaffStatus.Active)
            throw new DomainException("Staff user is not active");
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasPermission(string resource, AccessAction action)
    {
        return Roles.Any(role => role.HasPermission(resource, action));
    }

    private void ValidateRoleAssignment(Role role)
    {
        if (role.Level == AccessLevel.Brand && !BrandId.HasValue)
            throw new DomainException("Cannot assign brand role without brand");
        if (role.Level == AccessLevel.Store && !StoreId.HasValue)
            throw new DomainException("Cannot assign store role without store");
    }

    public void Validate()
    {
        PersonalInfo.Validate();
        if (!Roles.Any())
            throw new DomainException("Staff user must have at least one role");
    }
}

public enum StaffStatus
{
    Active,
    Inactive,
    Suspended
}
```

#### Role (Entity)

```csharp
public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public AccessLevel Level { get; private set; }
    public bool RequiresMfa { get; private set; }
    public HashSet<Permission> Permissions { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public void AddPermission(Permission permission)
    {
        if (permission.Level > Level)
            throw new DomainException("Permission level exceeds role level");
        Permissions.Add(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePermission(Permission permission)
    {
        Permissions.Remove(permission);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasPermission(string resource, AccessAction action)
    {
        return Permissions.Any(p => p.Resource == resource && p.Action == action);
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new DomainException("Role name is required");
        if (!Permissions.Any())
            throw new DomainException("Role must have at least one permission");
    }
}
```

#### Permission (Value Object)

```csharp
public record Permission
{
    public string Name { get; init; }
    public string Resource { get; init; }
    public AccessAction Action { get; init; }
    public AccessLevel Level { get; init; }
    public Dictionary<string, string> Constraints { get; init; }

    public Permission(string name, string resource, AccessAction action, AccessLevel level, Dictionary<string, string>? constraints = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Permission name is required");
        if (string.IsNullOrEmpty(resource))
            throw new DomainException("Permission resource is required");

        Name = name;
        Resource = resource;
        Action = action;
        Level = level;
        Constraints = constraints ?? new Dictionary<string, string>();
    }
}

public enum AccessLevel
{
    System = 0,
    Business = 1,
    Brand = 2,
    Store = 3
}

public enum AccessAction
{
    Create,
    Read,
    Update,
    Delete,
    Execute
}
```

#### Business Rules
- Staff may hold multiple roles; `Deny` permissions override `Allow`.
- MFA is mandatory for users with `Admin` role.

#### Domain Services
- `AuthenticationService`
- `RBACService`

#### 4.3.1 Authentication & Security

##### Session Management Rules
- Secure session creation
- Automatic session expiration
- Force logout on password change
- Multiple device session handling
- Session invalidation on security events

##### MFA Requirements
- Mandatory for users with Admin role
- TOTP-based authentication
- Backup codes for recovery
- Device registration and trust

##### Access Control Rules
- Staff may hold multiple roles; `Deny` permissions override `Allow`
- Staff can only access stores they are explicitly assigned to
- Password reset links expire after 24 hours
- Staff sessions are invalidated on password change
- Failed login attempts tracked and accounts locked after threshold

#### Business Rules
- Staff may hold multiple roles; `Deny` permissions override `Allow`.
- MFA is mandatory for users with `Admin` role.

#### Domain Services
- `AuthenticationService`
- `RBACService`
- `StaffAuthenticationService` - Staff-specific authentication logic

---

### 4.4 Business / Brand / Store Management Context 🏢

#### Business (Aggregate Root)

```csharp
public class Business
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public BusinessType Type { get; private set; }
    public string RegistrationNumber { get; private set; }
    public string TaxIdentifier { get; private set; }
    public Address BillingAddress { get; private set; }
    public string BillingEmail { get; private set; }
    public string BillingPhone { get; private set; }
    public BusinessStatus Status { get; private set; }
    public SubscriptionTier SubscriptionTier { get; private set; }
    public DateTime SubscriptionStartDate { get; private set; }
    public DateTime SubscriptionEndDate { get; private set; }
    public IList<Brand> Brands { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void AddBrand(Brand brand)
    {
        ValidateSubscription();
        ValidateBrandLimit();
        Brands.Add(brand);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpgradeSubscription(SubscriptionTier newTier, DateTime endDate)
    {
        if (newTier <= SubscriptionTier)
            throw new DomainException("Can only upgrade subscription tier");
        SubscriptionTier = newTier;
        SubscriptionEndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateSubscription()
    {
        if (DateTime.UtcNow > SubscriptionEndDate.AddDays(15))
            throw new DomainException("Subscription grace period exceeded");
    }

    private void ValidateBrandLimit()
    {
        var maxBrands = SubscriptionTier switch
        {
            SubscriptionTier.Basic => 1,
            SubscriptionTier.Premium => 5,
            SubscriptionTier.Enterprise => int.MaxValue,
            _ => throw new ArgumentException("Invalid tier")
        };
        
        if (Brands.Count >= maxBrands)
            throw new DomainException($"Tier limited to {maxBrands} brands");
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new DomainException("Business name is required");
        if (string.IsNullOrEmpty(BillingEmail))
            throw new DomainException("Billing email is required");
        ValidateSubscription();
    }
}

public enum BusinessType
{
    Corporation,
    Partnership,
    SoleProprietorship
}

public enum SubscriptionTier
{
    Basic,
    Premium,
    Enterprise
}

public enum BusinessStatus
{
    Active,
    Suspended,
    Terminated
}
```

#### Brand (Entity)

```csharp
public class Brand
{
    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public Uri? LogoUrl { get; private set; }
    public string Description { get; private set; }
    public ContactInfo Contact { get; private set; }
    public Address Address { get; private set; }
    public BrandStatus Status { get; private set; }
    public FraudPolicy? DefaultFraudPolicy { get; private set; }
    public IList<Store> Stores { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void AddStore(Store store, SubscriptionTier tier)
    {
        ValidateStoreLimit(tier);
        Stores.Add(store);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFraudPolicy(FraudPolicy policy)
    {
        DefaultFraudPolicy = policy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateStoreLimit(SubscriptionTier tier)
    {
        var maxStores = tier switch
        {
            SubscriptionTier.Basic => 5,
            SubscriptionTier.Premium => 20,
            SubscriptionTier.Enterprise => int.MaxValue,
            _ => throw new ArgumentException("Invalid tier")
        };
        
        if (Stores.Count >= maxStores)
            throw new DomainException($"Tier limited to {maxStores} stores");
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new DomainException("Brand name is required");
        Contact.Validate();
    }
}

public enum BrandStatus
{
    Active,
    Inactive
}
```

#### Store (Entity)

```csharp
public class Store
{
    public Guid Id { get; private set; }
    public Guid BrandId { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public GeoLocation Location { get; private set; }
    public OperatingHours Hours { get; private set; }
    public ContactInfo Contact { get; private set; }
    public StoreStatus Status { get; private set; }
    public FraudPolicy? PolicyOverride { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void UpdateLocation(GeoLocation location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateOperatingHours(OperatingHours hours)
    {
        Hours = hours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFraudPolicy(FraudPolicy policy)
    {
        PolicyOverride = policy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOpenAt(DateTime time)
    {
        return Hours.IsOpen(time);
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Name))
            throw new DomainException("Store name is required");
        Address.Validate();
        Contact.Validate();
    }
}

public enum StoreStatus
{
    Active,
    Inactive,
    Closed
}
```

#### Value Objects

```csharp
public record Address
{
    public string Line1 { get; init; }
    public string? Line2 { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Line1))
            throw new DomainException("Address line 1 is required");
        if (string.IsNullOrEmpty(City))
            throw new DomainException("City is required");
        if (string.IsNullOrEmpty(PostalCode))
            throw new DomainException("Postal code is required");
        if (string.IsNullOrEmpty(Country))
            throw new DomainException("Country is required");
    }
}

public record GeoLocation
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public GeoLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("Latitude must be between -90 and 90");
        if (longitude < -180 || longitude > 180)
            throw new DomainException("Longitude must be between -180 and 180");
        
        Latitude = latitude;
        Longitude = longitude;
    }
}

public record OperatingHours
{
    public Dictionary<DayOfWeek, TimeRange> Schedule { get; init; }
    
    public bool IsOpen(DateTime time)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
        return Schedule.TryGetValue(localTime.DayOfWeek, out var range) && 
               range.Contains(localTime.TimeOfDay);
    }

    public OperatingHours(Dictionary<DayOfWeek, TimeRange> schedule)
    {
        Schedule = schedule ?? throw new DomainException("Schedule is required");
    }
}

public record TimeRange
{
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
    
    public bool Contains(TimeSpan time) =>
        time >= Start && time <= End;

    public TimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new DomainException("Start time must be before end time");
        Start = start;
        End = end;
    }
}

public record ContactInfo
{
    public string Email { get; init; }
    public string Phone { get; init; }
    public Uri? Website { get; init; }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Email))
            throw new DomainException("Email is required");
        if (string.IsNullOrEmpty(Phone))
            throw new DomainException("Phone is required");
    }
}
```

#### Subscription Management

##### Subscription Tier Limits
- **Basic tier**: max 1 brand, 5 stores per brand
- **Premium tier**: max 5 brands, 20 stores per brand  
- **Enterprise tier**: unlimited brands and stores

##### Feature Availability
- **Basic**: Standard loyalty features only
- **Premium**: Advanced analytics, API access
- **Enterprise**: Custom features, dedicated support

##### Subscription Status Rules
- Active subscriptions required for program operations
- 15-day grace period for expired subscriptions
- Automatic suspension after grace period
- Data retained for 90 days after suspension

##### Tier Management Rules
- Upgrade effective immediately
- Downgrade effective next billing cycle
- Pro-rata billing for upgrades
- Migration plan required for downgrades

#### Subscription‑Related Invariants
- **Basic tier**: max 1 brand, 5 stores.  
- **Grace period**: 15 days before suspension.

#### Services
- `SubscriptionService`
- `BillingAdapter`

#### Domain Events
- `SubscriptionUpgraded`
- `SubscriptionExpired`

`OperatingHours` VO: list of `DayOfWeek`, open/close times.

`FraudPolicy` may be set at Store or Program; Brand/Business may define defaults.

Services & Events unchanged except:

- New event `StoreFraudPolicyUpdated`.

---

### 4.5 Integration Context 🔌

This context ingests purchase or visit information from **multiple hardware‑free channels** and converts them to a canonical command for the core domain.

#### Transaction Source Enumeration

```csharp
enum TransactionSource {
    PaymentProcessorWebhook, // Stripe / Square / Adyen signed payload
    PosAggregator,           // Omnivore, ShoppinPal, etc.
    QRCheckIn,               // Static counter QR
    StaffPwa,                // Manual staff entry via PWA
    EmailReceipt,            // Parsed digital receipt (BCC)
    BookingWebhook,          // Appointment SaaS (Fresha, Phorest)
    CardLink                 // Card‑linked offer networks (future)
}
```

#### 4.5.2 Adapter Services (Anti‑Corruption Layers)

| Adapter Service | External Trigger | Maps To | Idempotency Key |
|-----------------|------------------|---------|-----------------|
| `PaymentWebhookAdapter` | PSP webhook | `PaymentProcessorWebhook` | PSP event ID |
| `PosAggregatorAdapter` | Aggregator push | `PosAggregator` | Aggregator Txn GUID |
| `QRCodeCheckInService` | HTTPS from QR scan | `QRCheckIn` | `CardId + Timestamp` |
| `StaffPwaAdapter` | Auth'd POST | `StaffPwa` | Client‑generated UUID |
| `EmailReceiptParser` | SES inbound email | `EmailReceipt` | Message‑ID |
| `BookingWebhookAdapter` | Appointment completed | `BookingWebhook` | Booking ID |
| `CardLinkingAdapter` | Network cleared file | `CardLink` | PAN‑token + AuthCode |

All adapters publish `PurchaseCaptured` commands; duplicates are silently ignored by Loyalty context.


#### 4.5.3 Fraud / Abuse Controls

| Source | Mitigation |
|--------|------------|
| **QRCheckIn** | Same `CardId` cannot earn twice within configurable cooldown at the same `StoreId`. |
| **StaffPwa** | Requires `CanAwardManual` permission; every manual entry logged in `AuditEvent`. |
| **EmailReceipt** | DKIM/DMARC checks; high‑spam‑score messages quarantined. |

---

### 4.6 Notification Context 📧

| Entity | Purpose |
|--------|---------|
| `NotificationTemplate` | Multilingual message bodies |
| `NotificationLog` | Delivery audit |

#### 4.6.1 Email Service Requirements

##### Email Templates
- Verification Email
- Password Reset
- Welcome Email
- Account Changes
- Security Notifications

##### Delivery Tracking
- Sent Status
- Delivery Status
- Open Tracking
- Click Tracking
- Bounce Handling

##### Email Security
- Signed email links to prevent tampering
- Rate limiting for email-based actions
- Secure storage of email templates
- SPF and DKIM email authentication
- Monitoring for email abuse patterns

`NotificationOrchestrator` chooses best channel per `CommsPreferences`.  
Rule: no more than **3 emails / hour / customer**.

---

### 4.7 Audit & Analytics Context 📊

#### 4.7.1 Audit Requirements

##### Audit Events
- **System Changes**: Configuration updates, program modifications, rule changes, permission changes
- **User Actions**: Authentication attempts, data access, transaction processing, reward redemptions
- **Security Events**: Access violations, rate limit breaches, suspicious activities, system errors

##### Audit Retention Rules
- Transaction data: 7 years
- Security events: 2 years
- System changes: 3 years
- User actions: 1 year

##### Archival Process
- Daily audit aggregation
- Monthly data archival
- Yearly data summarization
- Compliance reporting

#### 4.7.2 Analytics Requirements

##### Business Analytics
- Revenue impact
- Program ROI
- Customer acquisition cost
- Customer lifetime value
- Cross-brand analytics
- Brand performance comparison
- Customer overlap analysis
- Program effectiveness
- Market penetration

##### Customer Analytics
- Behavioral analysis: Purchase patterns, reward preferences, program engagement, churn prediction
- Segmentation: Value-based segments, behavioral segments, program usage segments, cross-brand activity

##### Operational Analytics
- Program performance: Transaction volumes, reward redemption rates, point/stamp accumulation, program costs
- System performance: API usage, response times, error rates, integration health

- Append‑only `AuditEvent` store in S3 (7‑year retention for transactions).  
- `AnalyticsAPI` exposes pre‑aggregated KPIs (revenue impact, redemption rates).

---

## 5. Cross‑Cutting Concerns

| Concern | Approach |
|---------|----------|
| **Multi‑Currency** | `Money` VO; daily FX feed; amounts stored in original & base currency |
| **Rate Limiting** | NGINX + Redis leaky‑bucket keyed by `TenantId` |
| **Observability** | OpenTelemetry traces; JSON logs with Correlation‑ID |
| **Security** | JWT with tenant claims; OWASP CSP headers; SOC‑2 goals |
| **Authentication** | Multi-provider support (local, Google, Apple) with MFA |
| **Session Management** | Secure tokens with automatic expiration and device tracking |
| **Email Security** | DKIM/SPF validation, rate limiting, bounce handling |
| **Data Protection** | GDPR compliance, data retention policies, secure deletion |

---

## 6. Domain Events Catalogue 🌐

| Event | Source Context | Notes |
|-------|----------------|-------|
| `LoyaltyCardCreated` | Loyalty | CardID, CustomerID, ProgramID, ProgramType, Timestamp |
| `StampsIssued` | Loyalty | CardID, CustomerID, StampsIssued, TotalStamps, StoreID, Timestamp |
| `PointsAdded` | Loyalty | CardID, CustomerID, PointsAdded, PointsBalance, TransactionAmount, StoreID, Timestamp |
| `RewardRedeemed` | Loyalty | CardID, CustomerID, RewardID, RewardTitle, StoreID, Timestamp |
| `LoyaltyExpiringSoon` | Loyalty | CardID, CustomerID, ExpiringValue, ExpirationDate |
| `FraudAttemptDetected` | Loyalty / Integration | NEW – payload: reason, ruleId |
| `CustomerRegistered` | Customer | — |
| `CustomerDeleted` | Customer | — |
| `ConsentUpdated` | Customer | — |
| `EmailVerified` | Customer | UserID, UserType, EmailAddress, Timestamp |
| `PasswordReset` | Customer | UserID, UserType, Timestamp |
| `StaffLoggedIn` | Staff | — 
| `SubscriptionUpgraded` | Business | — |
| `SubscriptionExpired` | Business | — |
| `WebhookDeliveryFailed` | Integration | — |
| `PurchaseCaptured` | Integration | Canonical command |
| `NotificationSent` | Notification | — |

---

## 7. Ubiquitous Language Glossary 📚

| Term | Context | Meaning |
|------|---------|---------|
| **Tenant** | Platform | Top‑level Business account |
| **Brand** | Business | Public‑facing identity under a Business |
| **Store** | Business | Physical or online outlet |
| **Card Status** | Loyalty | `Active`, `Suspended`, `Expired` |
| **Reward Type** | Loyalty | `FreeItem`, `PercentageOff`, `MonetaryValue` |
| **Transaction Source** | Integration | Origin channel for a purchase event |
| **Adapter Service** | Integration | ACL translating external payload to domain command |
| **Audit Event** | Audit | Immutable log entry |
| **MFA** | Staff | Multi‑Factor Authentication |

---

## 8. Appendix A – Merchant Integration Options 🛍️

| # | Integration Style | Merchant Setup Effort | Customer Effort | Data Fidelity | Fraud Risk | Best For |
|---|-------------------|-----------------------|-----------------|--------------|------------|----------|
| **1** | Payment‑Processor Webhooks | Paste URL in Stripe/Square dashboard | None | Exact amount | Low | Merchants on modern PSPs |
| **2** | POS‑Aggregator SaaS | Subscribe & grant API key | None | Exact spend | Low | POS brands unsupported directly |
| **3** | Static Counter QR ("Check‑In") | Print & display QR | Scan once | 1 stamp | Medium | Pop‑ups, micro‑retail |
| **4** | Staff PWA Entry | Bookmark mobile form | None | Exact spend (typed) | Medium‑High | Salons, bars |
| **5** | Email‑Receipt Parsing | Add BCC address | None | Exact spend | Low‑Med | E‑commerce & email receipts |
| **6** | Booking‑System Webhook | OAuth connect | None | Exact spend | Low | Service businesses |
| **7** | Card‑Linked Offers (Future) | None | Link card once | Exact settled (D+1) | Very Low | Premium upsell |

---

## ✅ Completeness Checklist

| Item | Included |
|------|----------|
| FraudPolicy VO & rules | ✔ |
| Reward frequency controls | ✔ |
| Store geo/time rules | ✔ |
| FraudEvaluationService | ✔ |
| Domain event `FraudAttemptDetected` | ✔ |
| Updated specs & glossary | ✔ |

*End of Specification v0.5*
