-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'LoyaltySystem')
BEGIN
    CREATE DATABASE LoyaltySystem;
END
GO

USE LoyaltySystem;
GO

-- Create Brands table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brands]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Brands] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Category] NVARCHAR(50) NULL,
        [Logo] NVARCHAR(255) NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL
    );
END
GO

-- Create BrandContacts table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrandContacts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BrandContacts] (
        [BrandId] UNIQUEIDENTIFIER PRIMARY KEY,
        [Email] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(50) NULL,
        [Website] NVARCHAR(255) NULL,
        CONSTRAINT [FK_BrandContacts_Brands] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[Brands] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create BrandAddresses table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrandAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BrandAddresses] (
        [BrandId] UNIQUEIDENTIFIER PRIMARY KEY,
        [Line1] NVARCHAR(100) NOT NULL,
        [Line2] NVARCHAR(100) NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(50) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(50) NOT NULL,
        CONSTRAINT [FK_BrandAddresses_Brands] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[Brands] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create Stores table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Stores] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [BrandId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [ContactInfo] NVARCHAR(255) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_Stores_Brands] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[Brands] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create StoreAddresses table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StoreAddresses] (
        [StoreId] UNIQUEIDENTIFIER PRIMARY KEY,
        [Line1] NVARCHAR(100) NOT NULL,
        [Line2] NVARCHAR(100) NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(50) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(50) NOT NULL,
        CONSTRAINT [FK_StoreAddresses_Stores] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create StoreGeoLocations table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreGeoLocations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StoreGeoLocations] (
        [StoreId] UNIQUEIDENTIFIER PRIMARY KEY,
        [Latitude] FLOAT NOT NULL,
        [Longitude] FLOAT NOT NULL,
        CONSTRAINT [FK_StoreGeoLocations_Stores] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create StoreOperatingHours table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreOperatingHours]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StoreOperatingHours] (
        [StoreId] UNIQUEIDENTIFIER NOT NULL,
        [DayOfWeek] INT NOT NULL,
        [OpenTime] TIME NOT NULL,
        [CloseTime] TIME NOT NULL,
        CONSTRAINT [PK_StoreOperatingHours] PRIMARY KEY ([StoreId], [DayOfWeek]),
        CONSTRAINT [FK_StoreOperatingHours_Stores] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create Customers table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Customers] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(100) NULL,
        [Phone] NVARCHAR(50) NULL,
        [MarketingConsent] BIT NOT NULL DEFAULT 0,
        [JoinedAt] DATETIME2 NOT NULL,
        [LastLoginAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL
    );
    
    CREATE INDEX [IX_Customers_Email] ON [dbo].[Customers] ([Email]) WHERE [Email] IS NOT NULL;
    CREATE INDEX [IX_Customers_Phone] ON [dbo].[Customers] ([Phone]) WHERE [Phone] IS NOT NULL;
END
GO

-- Create LoyaltyPrograms table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyPrograms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyPrograms] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [BrandId] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Type] INT NOT NULL, -- 1=Stamp, 2=Points
        [StampThreshold] INT NULL,
        [PointsConversionRate] DECIMAL(18, 2) NULL,
        [DailyStampLimit] INT NULL,
        [MinimumTransactionAmount] DECIMAL(18, 2) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_LoyaltyPrograms_Brands] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[Brands] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_LoyaltyPrograms_BrandId] ON [dbo].[LoyaltyPrograms] ([BrandId]);
    CREATE INDEX [IX_LoyaltyPrograms_Type] ON [dbo].[LoyaltyPrograms] ([Type]);
END
GO

-- Create ProgramExpirationPolicies table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProgramExpirationPolicies]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProgramExpirationPolicies] (
        [ProgramId] UNIQUEIDENTIFIER PRIMARY KEY,
        [HasExpiration] BIT NOT NULL DEFAULT 0,
        [ExpirationType] INT NULL, -- 1=Days, 2=Months, 3=Years
        [ExpirationValue] INT NULL,
        [ExpiresOnSpecificDate] BIT NOT NULL DEFAULT 0,
        [ExpirationDay] INT NULL,
        [ExpirationMonth] INT NULL,
        CONSTRAINT [FK_ProgramExpirationPolicies_LoyaltyPrograms] FOREIGN KEY ([ProgramId]) REFERENCES [dbo].[LoyaltyPrograms] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Create Rewards table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Rewards]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Rewards] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [ProgramId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [RequiredValue] INT NOT NULL,
        [ValidFrom] DATETIME2 NULL,
        [ValidTo] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_Rewards_LoyaltyPrograms] FOREIGN KEY ([ProgramId]) REFERENCES [dbo].[LoyaltyPrograms] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Rewards_ProgramId] ON [dbo].[Rewards] ([ProgramId]);
    CREATE INDEX [IX_Rewards_ValidPeriod] ON [dbo].[Rewards] ([ValidFrom], [ValidTo]);
END
GO

-- Create LoyaltyCards table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyCards]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyCards] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [ProgramId] UNIQUEIDENTIFIER NOT NULL,
        [CustomerId] UNIQUEIDENTIFIER NOT NULL,
        [Type] INT NOT NULL, -- 1=Stamp, 2=Points
        [StampsCollected] INT NOT NULL DEFAULT 0,
        [PointsBalance] DECIMAL(18, 2) NOT NULL DEFAULT 0,
        [Status] INT NOT NULL, -- 1=Active, 2=Expired, 3=Suspended
        [QrCode] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ExpiresAt] DATETIME2 NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_LoyaltyCards_LoyaltyPrograms] FOREIGN KEY ([ProgramId]) REFERENCES [dbo].[LoyaltyPrograms] ([Id]),
        CONSTRAINT [FK_LoyaltyCards_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([Id])
    );
    
    CREATE UNIQUE INDEX [IX_LoyaltyCards_QrCode] ON [dbo].[LoyaltyCards] ([QrCode]);
    CREATE INDEX [IX_LoyaltyCards_ProgramId] ON [dbo].[LoyaltyCards] ([ProgramId]);
    CREATE INDEX [IX_LoyaltyCards_CustomerId] ON [dbo].[LoyaltyCards] ([CustomerId]);
    CREATE INDEX [IX_LoyaltyCards_Status] ON [dbo].[LoyaltyCards] ([Status]);
    CREATE INDEX [IX_LoyaltyCards_ExpiresAt] ON [dbo].[LoyaltyCards] ([ExpiresAt]) WHERE [ExpiresAt] IS NOT NULL;
END
GO

-- Create Transactions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Transactions] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [CardId] UNIQUEIDENTIFIER NOT NULL,
        [Type] INT NOT NULL, -- 1=StampIssuance, 2=PointsIssuance, 3=RewardRedemption, 4=StampVoid, 5=PointsVoid
        [RewardId] UNIQUEIDENTIFIER NULL,
        [Quantity] INT NULL,
        [PointsAmount] DECIMAL(18, 2) NULL,
        [TransactionAmount] DECIMAL(18, 2) NULL,
        [StoreId] UNIQUEIDENTIFIER NOT NULL,
        [StaffId] UNIQUEIDENTIFIER NULL,
        [PosTransactionId] NVARCHAR(100) NULL,
        [Timestamp] DATETIME2 NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [Metadata] NVARCHAR(MAX) NULL,
        CONSTRAINT [FK_Transactions_LoyaltyCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[LoyaltyCards] ([Id]),
        CONSTRAINT [FK_Transactions_Rewards] FOREIGN KEY ([RewardId]) REFERENCES [dbo].[Rewards] ([Id]),
        CONSTRAINT [FK_Transactions_Stores] FOREIGN KEY ([StoreId]) REFERENCES [dbo].[Stores] ([Id])
    );
    
    CREATE INDEX [IX_Transactions_CardId] ON [dbo].[Transactions] ([CardId]);
    CREATE INDEX [IX_Transactions_Type] ON [dbo].[Transactions] ([Type]);
    CREATE INDEX [IX_Transactions_Timestamp] ON [dbo].[Transactions] ([Timestamp]);
    CREATE INDEX [IX_Transactions_StoreId] ON [dbo].[Transactions] ([StoreId]);
    CREATE INDEX [IX_Transactions_PosTransactionId] ON [dbo].[Transactions] ([PosTransactionId]) WHERE [PosTransactionId] IS NOT NULL;
END
GO

-- Create CardLinks table for POS integration
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CardLinks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CardLinks] (
        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
        [CardId] UNIQUEIDENTIFIER NOT NULL,
        [CardHash] NVARCHAR(255) NOT NULL,
        [LinkType] INT NOT NULL, -- 1=PaymentCard, 2=PhoneNumber, 3=Email, 4=Other
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [FK_CardLinks_LoyaltyCards] FOREIGN KEY ([CardId]) REFERENCES [dbo].[LoyaltyCards] ([Id])
    );
    
    CREATE UNIQUE INDEX [IX_CardLinks_CardHash] ON [dbo].[CardLinks] ([CardHash]);
    CREATE INDEX [IX_CardLinks_CardId] ON [dbo].[CardLinks] ([CardId]);
END
GO 