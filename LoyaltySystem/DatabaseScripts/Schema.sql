-----------------------------
-- Drop Existing Tables (if any)
-----------------------------
DROP TABLE IF EXISTS Promotions CASCADE;
DROP TABLE IF EXISTS RedemptionTransactions CASCADE;
DROP TABLE IF EXISTS LoyaltyCardRewardMappings CASCADE;
DROP TABLE IF EXISTS LoyaltyProgramSettings CASCADE;
DROP TABLE IF EXISTS Rewards CASCADE;
DROP TABLE IF EXISTS StampTransactions CASCADE;
DROP TABLE IF EXISTS UserLoyaltyCards CASCADE;
DROP TABLE IF EXISTS LoyaltyCardTemplates CASCADE;
DROP TABLE IF EXISTS QRCodeDesigns CASCADE;
DROP TABLE IF EXISTS FraudSettings CASCADE;
DROP TABLE IF EXISTS Stores CASCADE;
DROP TABLE IF EXISTS BusinessUsers CASCADE;
DROP TABLE IF EXISTS Businesses CASCADE;
DROP TABLE IF EXISTS Users CASCADE;
DROP TABLE IF EXISTS Members CASCADE;

-----------------------------
-- Create Users Table
-----------------------------
CREATE TABLE Users (
UserId SERIAL PRIMARY KEY,
Email VARCHAR(256) NOT NULL UNIQUE,
PasswordHash VARCHAR(512) NOT NULL,
FirstName VARCHAR(100) NOT NULL,
LastName VARCHAR(100) NOT NULL,
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-----------------------------
-- Create Business Table
-----------------------------
CREATE TABLE Businesses (
BusinessId SERIAL PRIMARY KEY,
BusinessName VARCHAR(200) NOT NULL,
Category VARCHAR(100),
WebsiteUrl VARCHAR(256),
Description TEXT,
LogoUrl VARCHAR(256),
CoverImageUrl VARCHAR(256),
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-----------------------------
-- Create BusinessUser Join Table
-----------------------------
CREATE TABLE BusinessUsers (
BusinessUserId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
UserId INT NOT NULL,
Role VARCHAR(50) NOT NULL,  -- e.g., 'BusinessOwner', 'StaffAdmin', 'StaffViewer'
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_BusinessUser_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId),
CONSTRAINT FK_BusinessUser_User FOREIGN KEY (UserId)
REFERENCES Users(UserId)
);

-----------------------------
-- Create Store Table
-----------------------------
CREATE TABLE Stores (
StoreId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
StoreName VARCHAR(200) NOT NULL,
PhoneNumber VARCHAR(50),
Address VARCHAR(256) NOT NULL,
Postcode VARCHAR(50) NOT NULL,
QRCodeData TEXT,  -- Data needed to generate/display QR codes
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_Store_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId)
);

-----------------------------
-- Create FraudSettings Table
-----------------------------
CREATE TABLE FraudSettings (
FraudSettingID SERIAL PRIMARY KEY,
StoreId INT NOT NULL,
AllowedLatitude DECIMAL(9,6) NOT NULL,
AllowedLongitude DECIMAL(9,6) NOT NULL,
AllowedRadius INT NOT NULL,  -- in meters
OperationalStart TIME NOT NULL,  -- e.g., '09:00'
OperationalEnd TIME NOT NULL,    -- e.g., '21:00'
MinIntervalMinutes INT NOT NULL DEFAULT 240,
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_FraudSettings_Store FOREIGN KEY (StoreId)
REFERENCES Stores(StoreId)
);

-----------------------------
-- Create QRCodeDesign Table
-----------------------------
CREATE TABLE QRCodeDesigns (
QRCodeDesignId SERIAL PRIMARY KEY,
BusinessId INT,  -- Can be scoped at the business level
StoreId INT,     -- or at the store level
DesignSettings TEXT NOT NULL,  -- JSON or XML with design details
LastUpdated TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_QRCodeDesign_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId),
CONSTRAINT FK_QRCodeDesign_Store FOREIGN KEY (StoreId)
REFERENCES Stores(StoreId)
);

-----------------------------
-- Create LoyaltyCardTemplate Table
-----------------------------
CREATE TABLE LoyaltyCardTemplates (
LoyaltyCardTemplateId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
CardType VARCHAR(50) NOT NULL,         -- e.g., 'RegularStampCard', 'DigitalStampPassport'
CardName VARCHAR(100) NOT NULL,
RequiredStamps INT NOT NULL,
MinimumSpendCondition DECIMAL(10,2),
Description TEXT,
ResetAfterCompletion BOOLEAN NOT NULL DEFAULT FALSE,
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_LoyaltyCardTemplate_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId)
);

-----------------------------
-- Create UserLoyaltyCard Table
-----------------------------
CREATE TABLE UserLoyaltyCards (
UserLoyaltyCardId SERIAL PRIMARY KEY,
UserId INT NOT NULL,
LoyaltyCardTemplateId INT NOT NULL,
BusinessId INT NOT NULL,  -- Redundant for quick filtering
CurrentStampCount INT NOT NULL DEFAULT 0,
Status VARCHAR(50) NOT NULL,  -- e.g., 'Active', 'Completed'
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_UserLoyaltyCard_User FOREIGN KEY (UserId)
REFERENCES Users(UserId),
CONSTRAINT FK_UserLoyaltyCard_LoyaltyCardTemplate FOREIGN KEY (LoyaltyCardTemplateId)
REFERENCES LoyaltyCardTemplates(LoyaltyCardTemplateId),
CONSTRAINT FK_UserLoyaltyCard_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId)
);

-----------------------------
-- Create StampTransaction Table
-----------------------------
CREATE TABLE StampTransactions (
StampTransactionId SERIAL PRIMARY KEY,
UserLoyaltyCardId INT NOT NULL,
StoreId INT NOT NULL,
Timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
GeoLocation VARCHAR(100),  -- e.g., "lat,lon" format
CONSTRAINT FK_StampTransaction_UserLoyaltyCard FOREIGN KEY (UserLoyaltyCardId)
REFERENCES UserLoyaltyCards(UserLoyaltyCardId),
CONSTRAINT FK_StampTransaction_Store FOREIGN KEY (StoreId)
REFERENCES Stores(StoreId)
);

-----------------------------
-- Create Reward Table
-----------------------------
CREATE TABLE Rewards (
RewardId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
RewardTitle VARCHAR(200) NOT NULL,
WhatYouGet TEXT NOT NULL,  -- Description of reward
FinePrint TEXT,
RewardImageUrl VARCHAR(256),
IsGift BOOLEAN NOT NULL DEFAULT FALSE,
RewardType VARCHAR(50) NOT NULL,     -- e.g., 'Regular', 'Birthday', 'Welcome', 'Referral'
ValidityStart TIMESTAMP,
ValidityEnd TIMESTAMP,
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_Reward_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId)
);

-----------------------------
-- Create LoyaltyProgramSettings Table
-----------------------------
CREATE TABLE LoyaltyProgramSettings (
SettingsId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
BirthdayRewardId INT,    -- Optional FK to Reward
WelcomeRewardId INT,     -- Optional FK to Reward
ReferralProgramDetails TEXT,  -- JSON for custom referral details
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_LoyaltyProgramSettings_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId),
CONSTRAINT FK_LoyaltyProgramSettings_BirthdayReward FOREIGN KEY (BirthdayRewardId)
REFERENCES Rewards(RewardId),
CONSTRAINT FK_LoyaltyProgramSettings_WelcomeReward FOREIGN KEY (WelcomeRewardId)
REFERENCES Rewards(RewardId)
);

-----------------------------
-- Create LoyaltyCardRewardMapping Table
-----------------------------
CREATE TABLE LoyaltyCardRewardMappings (
MappingId SERIAL PRIMARY KEY,
LoyaltyCardTemplateId INT NOT NULL,
RewardId INT NOT NULL,
RequiredStampsForReward INT NOT NULL,
CONSTRAINT FK_LoyaltyCardRewardMapping_LoyaltyCardTemplate FOREIGN KEY (LoyaltyCardTemplateId)
REFERENCES LoyaltyCardTemplates(LoyaltyCardTemplateId),
CONSTRAINT FK_LoyaltyCardRewardMapping_Reward FOREIGN KEY (RewardId)
REFERENCES Rewards(RewardId)
);

-----------------------------
-- Create RedemptionTransaction Table
-----------------------------
CREATE TABLE RedemptionTransactions (
RedemptionTransactionId SERIAL PRIMARY KEY,
UserLoyaltyCardId INT NOT NULL,
RewardId INT NOT NULL,
StoreId INT NOT NULL,
Timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_RedemptionTransaction_UserLoyaltyCard FOREIGN KEY (UserLoyaltyCardId)
REFERENCES UserLoyaltyCards(UserLoyaltyCardId),
CONSTRAINT FK_RedemptionTransaction_Reward FOREIGN KEY (RewardId)
REFERENCES Rewards(RewardId),
CONSTRAINT FK_RedemptionTransaction_Store FOREIGN KEY (StoreId)
REFERENCES Stores(StoreId)
);

-----------------------------
-- Create Promotion Table
-----------------------------
CREATE TABLE Promotions (
PromotionId SERIAL PRIMARY KEY,
BusinessId INT NOT NULL,
Title VARCHAR(200) NOT NULL,
Description TEXT,
ValidFrom TIMESTAMP NOT NULL,
ValidUntil TIMESTAMP NOT NULL,
CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_Promotion_Business FOREIGN KEY (BusinessId)
REFERENCES Businesses(BusinessId)
);


CREATE TABLE Members (
    MemberId     SERIAL PRIMARY KEY,
    BusinessId   INT NOT NULL,
    Name         VARCHAR(100) NOT NULL,
    Email        VARCHAR(256) NOT NULL,
    JoinedAt     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- any other columns you need for your members
    -- e.g. phone, marketing preferences, etc.
    
    -- Foreign key to the Businesses table
    CONSTRAINT FK_Members_Businesses FOREIGN KEY (BusinessId)
    REFERENCES Businesses(BusinessId)
);