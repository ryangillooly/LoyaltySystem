BEGIN;

-- Set client_min_messages to suppress notices during development
SET client_min_messages TO warning;

-- Create custom types using PostgreSQL ENUM
DO $$
BEGIN
BEGIN
CREATE TYPE loyalty_program_type AS ENUM ('Stamp', 'Points');
CREATE TYPE transaction_type AS ENUM ('StampIssuance', 'PointsIssuance', 'RewardRedemption', 'StampVoid', 'PointsVoid');
CREATE TYPE card_status AS ENUM ('Active', 'Expired', 'Suspended');
CREATE TYPE expiration_type AS ENUM ('Days', 'Months', 'Years');
CREATE TYPE card_link_type AS ENUM ('PaymentCard', 'PhoneNumber', 'Email', 'Other');
EXCEPTION WHEN duplicate_object THEN
        RAISE NOTICE 'Types already exist, continuing...';
END;
END
$$;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create functions and triggers separately (outside the main DO block)
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION check_card_program_type_consistency()
RETURNS TRIGGER AS $$
DECLARE
program_type loyalty_program_type;
BEGIN
SELECT type INTO program_type
FROM loyalty_programs
WHERE id = NEW.program_id;

IF NEW.type != program_type THEN
        RAISE EXCEPTION 'Card type must match program type';
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION validate_reward_redemption()
RETURNS TRIGGER AS $$
DECLARE
card_record RECORD;
    reward_record RECORD;
BEGIN
    IF NEW.type != 'RewardRedemption' THEN
        RETURN NEW;
END IF;
    
    -- Get card details
SELECT * INTO card_record
FROM loyalty_cards
WHERE id = NEW.card_id;

-- Get reward details
SELECT * INTO reward_record
FROM rewards
WHERE id = NEW.reward_id;

-- Check if reward belongs to the card's program
IF card_record.program_id != reward_record.program_id THEN
        RAISE EXCEPTION 'Reward does not belong to the card program';
END IF;
    
    -- Check if card has enough points/stamps
    IF card_record.type = 'Stamp' AND card_record.stamps_collected < reward_record.required_value THEN
        RAISE EXCEPTION 'Insufficient stamps for redemption';
    ELSIF card_record.type = 'Points' AND card_record.points_balance < reward_record.required_value THEN
        RAISE EXCEPTION 'Insufficient points for redemption';
END IF;
    
    -- Check reward validity period
    IF (reward_record.valid_from IS NOT NULL AND reward_record.valid_from > CURRENT_TIMESTAMP) OR
       (reward_record.valid_to IS NOT NULL AND reward_record.valid_to < CURRENT_TIMESTAMP) THEN
        RAISE EXCEPTION 'Reward is not valid at this time';
END IF;
    
    -- Check if reward is active
    IF NOT reward_record.is_active THEN
        RAISE EXCEPTION 'Reward is not active';
END IF;

RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create app context functions
CREATE OR REPLACE FUNCTION set_app_context(p_key TEXT, p_value TEXT)
RETURNS VOID AS $$
BEGIN
    -- Delete existing key if present
DELETE FROM app_context WHERE key = p_key;

-- Insert new key-value
INSERT INTO app_context (key, value) VALUES (p_key, p_value);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_app_context(p_key TEXT)
RETURNS TEXT AS $$
DECLARE
v_value TEXT;
BEGIN
SELECT value INTO v_value FROM app_context WHERE key = p_key;
RETURN v_value;
END;
$$ LANGUAGE plpgsql;

-- Create partition management procedures
CREATE OR REPLACE PROCEDURE create_transaction_partition(year INTEGER)
LANGUAGE plpgsql
AS $$
BEGIN
EXECUTE format(
        'CREATE TABLE IF NOT EXISTS transactions_%s PARTITION OF transactions
        FOR VALUES FROM (%L) TO (%L)',
    year,
    year || '-01-01',
        (year + 1) || '-01-01'
        );

-- Log it
INSERT INTO partition_maintenance_log (year, notes)
VALUES (year, 'Created partition for year ' || year);
END;
$$;

-- Define procedure for refreshing materialized views as a function instead
DROP PROCEDURE IF EXISTS refresh_analytics_views;
DROP FUNCTION IF EXISTS refresh_analytics_views();
CREATE OR REPLACE FUNCTION refresh_analytics_views()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW mv_program_metrics;
    REFRESH MATERIALIZED VIEW mv_customer_metrics;
END
$$;

-- Now create all the tables and structure in a single DO block
DO $$
BEGIN
    -- Create Businesses table
CREATE TABLE IF NOT EXISTS businesses 
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    tax_id VARCHAR(50) NULL,
    logo VARCHAR(255) NULL,
    website VARCHAR(255) NULL,
    founded_date DATE NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create BusinessContacts table
CREATE TABLE IF NOT EXISTS business_contacts (
                                                 business_id UUID PRIMARY KEY,
                                                 email VARCHAR(100) NULL,
    phone VARCHAR(50) NULL,
    website VARCHAR(255) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_business_contacts_businesses FOREIGN KEY (business_id) REFERENCES businesses (id) ON DELETE CASCADE
    );

-- Create BusinessAddresses table
CREATE TABLE IF NOT EXISTS business_addresses (
                                                  business_id UUID PRIMARY KEY,
                                                  line1 VARCHAR(100) NOT NULL,
    line2 VARCHAR(100) NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NULL,
    postal_code VARCHAR(20) NULL,
    country VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_business_addresses_businesses FOREIGN KEY (business_id) REFERENCES businesses (id) ON DELETE CASCADE
    );

-- Create Brands table with business_id foreign key
CREATE TABLE IF NOT EXISTS brands 
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    business_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    category VARCHAR(50) NULL,
    logo VARCHAR(255) NULL,
    description VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_brands_businesses FOREIGN KEY (business_id) REFERENCES businesses (id) ON DELETE CASCADE
    );

-- Create BrandContacts table
CREATE TABLE IF NOT EXISTS brand_contacts (
                                              brand_id UUID PRIMARY KEY,
                                              email VARCHAR(100) NULL,
    phone VARCHAR(50) NULL,
    website VARCHAR(255) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_brand_contacts_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
    );

-- Create BrandAddresses table
CREATE TABLE IF NOT EXISTS brand_addresses (
                                               brand_id UUID PRIMARY KEY,
                                               line1 VARCHAR(100) NOT NULL,
    line2 VARCHAR(100) NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NULL,
    postal_code VARCHAR(20) NULL,
    country VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_brand_addresses_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
    );

-- Create Stores table
CREATE TABLE IF NOT EXISTS stores 
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    brand_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    opening_hours JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_stores_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_stores_brand_id ON stores (brand_id);

-- Create StoreContacts table
CREATE TABLE IF NOT EXISTS store_contacts (
                                              store_id UUID PRIMARY KEY,
                                              email VARCHAR(100) NULL,
    phone VARCHAR(50) NULL,
    website VARCHAR(255) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_store_contacts_stores FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
    );

-- Create StoreAddresses table
CREATE TABLE IF NOT EXISTS store_addresses (
                                               store_id UUID PRIMARY KEY,
                                               location GEOGRAPHY(POINT) NOT NULL,
    line1 VARCHAR(100) NOT NULL,
    line2 VARCHAR(100) NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NULL,
    postal_code VARCHAR(20) NULL,
    country VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_store_addresses_stores FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
    );

-- Create spatial index
CREATE INDEX IF NOT EXISTS idx_store_addresses_location ON store_addresses USING GIST (location);

-- Create LoyaltyPrograms table
CREATE TABLE IF NOT EXISTS loyalty_programs
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    brand_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    type loyalty_program_type NOT NULL,
    stamp_threshold INTEGER NULL,
    points_conversion_rate NUMERIC(18, 2) NULL,
    daily_stamp_limit INTEGER NULL,
    minimum_transaction_amount NUMERIC(18, 2) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(255) NULL,
    points_rounding_rule SMALLINT NULL,
    enrollment_bonus_points INT NULL,
    minimum_points_for_redemption INT NULL,
    points_per_pound DECIMAL(10, 2) NULL,
    start_date TIMESTAMP NOT NULL,
    has_tiers BOOLEAN NOT NULL DEFAULT FALSE,
    end_date TIMESTAMP NOT NULL,
    terms_and_conditions VARCHAR(255) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_loyalty_programs_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE,
    CONSTRAINT chk_stamp_threshold CHECK (type != 'Stamp' OR stamp_threshold IS NOT NULL),
    CONSTRAINT chk_points_conversion CHECK (type != 'Points' OR points_conversion_rate IS NOT NULL)
    );

CREATE UNIQUE INDEX IF NOT EXISTS uix_loyalty_programs_prefixedid ON loyalty_programs(prefixed_id);

-- Create loyalty program tiers table
CREATE TABLE loyalty_program_tiers
(
    id UUID PRIMARY KEY,
    program_id UUID NOT NULL REFERENCES loyalty_programs(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    point_threshold INTEGER NOT NULL,
    point_multiplier DECIMAL(5, 2) NOT NULL,
    tier_order INTEGER NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_program_tier_order UNIQUE(program_id, tier_order)
);

CREATE INDEX IF NOT EXISTS idx_loyalty_program_tiers_program_id ON loyalty_program_tiers(program_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_programs_brand_id ON loyalty_programs (brand_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_programs_type ON loyalty_programs (type);

-- Create loyalty tier benefits table
CREATE TABLE IF NOT EXISTS loyalty_tier_benefits
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    tier_id UUID NOT NULL REFERENCES loyalty_program_tiers(id) ON DELETE CASCADE,
    benefit_description TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_loyalty_tier_benefits_tier_id ON loyalty_tier_benefits(tier_id);

-- Create ProgramExpirationPolicies table
CREATE TABLE IF NOT EXISTS program_expiration_policies
(
    program_id UUID PRIMARY KEY,
    has_expiration BOOLEAN NOT NULL DEFAULT FALSE,
    expiration_type expiration_type NULL,
    expiration_value INTEGER NULL,
    expires_on_specific_date BOOLEAN NOT NULL DEFAULT FALSE,
    expiration_day INTEGER NULL,
    expiration_month INTEGER NULL,
    CONSTRAINT fk_program_expiration_policies_loyalty_programs FOREIGN KEY (program_id) REFERENCES loyalty_programs (id) ON DELETE CASCADE,
    CONSTRAINT chk_expiration_type CHECK (has_expiration = FALSE OR expiration_type IS NOT NULL),
    CONSTRAINT chk_expiration_value CHECK (has_expiration = FALSE OR expiration_value IS NOT NULL),
    CONSTRAINT chk_expiration_day CHECK (expires_on_specific_date = FALSE OR expiration_day BETWEEN 1 AND 31),
    CONSTRAINT chk_expiration_month CHECK (expires_on_specific_date = FALSE OR expiration_month BETWEEN 1 AND 12)
    );

-- Create Rewards table
CREATE TABLE IF NOT EXISTS rewards
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    program_id UUID NOT NULL,
    title VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    required_value INTEGER NOT NULL,
    valid_from TIMESTAMP NULL,
    valid_to TIMESTAMP NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_rewards_loyalty_programs FOREIGN KEY (program_id) REFERENCES loyalty_programs (id) ON DELETE CASCADE,
    CONSTRAINT chk_valid_period CHECK (valid_from IS NULL OR valid_to IS NULL OR valid_from < valid_to)
    );

CREATE INDEX IF NOT EXISTS idx_rewards_program_id ON rewards (program_id);
CREATE INDEX IF NOT EXISTS idx_rewards_valid_period ON rewards (valid_from, valid_to) WHERE valid_from IS NOT NULL AND valid_to IS NOT NULL;
-- Add index for active rewards (commonly queried)
CREATE INDEX IF NOT EXISTS idx_rewards_program_active ON rewards(program_id, is_active) WHERE is_active = TRUE;

-- MOVED HERE: Create users table BEFORE transactions table
CREATE TABLE IF NOT EXISTS users 
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone VARCHAR(30) NOT NULL UNIQUE,
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE;
    password_hash VARCHAR(255) NOT NULL,
    status INT NOT NULL DEFAULT 1, -- 1=Active, 2=Inactive, 3=Locked, etc. (from UserStatus enum)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_phone ON users(phone);

-- Create Customers table
CREATE TABLE IF NOT EXISTS customers
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    prefixed_id VARCHAR(35) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone VARCHAR(30) NOT NULL UNIQUE,
    marketing_consent BOOLEAN NOT NULL DEFAULT FALSE,
    joined_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_customers_users FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_customers_email ON customers (email) WHERE email IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_customers_phone ON customers (phone) WHERE phone IS NOT NULL;
-- Add enhanced search for customers by name (using trigram similarity)
CREATE INDEX IF NOT EXISTS idx_customers_name_gin ON customers USING gin(first_name gin_trgm_ops, last_name gin_trgm_ops);
CREATE UNIQUE INDEX IF NOT EXISTS uix_customers_prefixedid ON customers(prefixed_id);

-- Create LoyaltyCards table
CREATE TABLE IF NOT EXISTS loyalty_cards
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    program_id UUID NOT NULL REFERENCES loyalty_programs(id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE CASCADE,
    type loyalty_program_type NOT NULL,
    stamps_collected INTEGER NOT NULL DEFAULT 0,
    points_balance NUMERIC(18, 2) NOT NULL DEFAULT 0,
    status card_status NOT NULL DEFAULT 'Active',
    qr_code VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_loyalty_cards_loyalty_programs FOREIGN KEY (program_id) REFERENCES loyalty_programs (id),
    CONSTRAINT fk_loyalty_cards_customers FOREIGN KEY (customer_id) REFERENCES customers (id),
    CONSTRAINT chk_stamps CHECK (type != 'Stamp' OR stamps_collected >= 0),
    CONSTRAINT chk_points CHECK (type != 'Points' OR points_balance >= 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS uix_loyalty_cards_prefixedid ON loyalty_cards(prefixed_id);
CREATE UNIQUE INDEX IF NOT EXISTS idx_loyalty_cards_qr_code ON loyalty_cards (qr_code);
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_program_id ON loyalty_cards (program_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_customer_id ON loyalty_cards (customer_id);
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_status ON loyalty_cards (status);
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_expires_at ON loyalty_cards (expires_at) WHERE expires_at IS NOT NULL;

-- Add composite index for common query patterns
CREATE INDEX IF NOT EXISTS idx_loyalty_cards_program_status ON loyalty_cards(program_id, status);

-- Create CardsLinks table
CREATE TABLE IF NOT EXISTS card_links
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    card_id UUID NOT NULL,
    card_hash VARCHAR(255) NOT NULL,
    link_type card_link_type NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_card_links_loyalty_cards FOREIGN KEY (card_id) REFERENCES loyalty_cards (id)
    );

CREATE UNIQUE INDEX IF NOT EXISTS idx_card_links_card_hash ON card_links (card_hash);
CREATE INDEX IF NOT EXISTS idx_card_links_card_id ON card_links (card_id);

-- Create Transactions table with partitioning
CREATE TABLE IF NOT EXISTS transactions
(
    id UUID DEFAULT uuid_generate_v4(),
    prefixed_id VARCHAR(35) NOT NULL,
    card_id UUID NOT NULL REFERENCES loyalty_cards(id) ON DELETE RESTRICT,
    type transaction_type NOT NULL,
    reward_id UUID NULL REFERENCES rewards(id) ON DELETE SET NULL,
    quantity INTEGER NULL,
    points_amount NUMERIC(18, 2) NULL,
    transaction_amount NUMERIC(18, 2) NULL,
    store_id UUID NOT NULL REFERENCES stores(id) ON DELETE SET NULL,
    staff_id UUID NULL REFERENCES users(id) ON DELETE SET NULL,
    pos_transaction_id VARCHAR(100) NULL,
    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB NULL,
    CONSTRAINT pk_transactions PRIMARY KEY (id, timestamp),
    CONSTRAINT fk_transactions_loyalty_cards FOREIGN KEY (card_id) REFERENCES loyalty_cards (id),
    CONSTRAINT fk_transactions_rewards FOREIGN KEY (reward_id) REFERENCES rewards (id),
    CONSTRAINT fk_transactions_stores FOREIGN KEY (store_id) REFERENCES stores (id),
    CONSTRAINT chk_reward_redemption CHECK (type != 'RewardRedemption' OR reward_id IS NOT NULL),
    CONSTRAINT chk_quantity CHECK (type != 'StampIssuance' OR quantity IS NOT NULL),
    CONSTRAINT chk_points CHECK (type != 'PointsIssuance' OR points_amount IS NOT NULL)
    ) PARTITION BY RANGE (timestamp);

-- Modified UNIQUE index to include the partitioning key 'timestamp'
CREATE UNIQUE INDEX IF NOT EXISTS uix_transactions_prefixedid ON transactions(prefixed_id, timestamp);

-- Create partitions for transactions (past year and current year)
CREATE TABLE IF NOT EXISTS transactions_2023 PARTITION OF transactions
    FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');

CREATE TABLE IF NOT EXISTS transactions_2024 PARTITION OF transactions
    FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');

-- Add partition for next year
CREATE TABLE IF NOT EXISTS transactions_2025 PARTITION OF transactions
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

-- Create indexes for transactions
CREATE INDEX IF NOT EXISTS idx_transactions_card_id ON transactions (card_id);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON transactions (type);
CREATE INDEX IF NOT EXISTS idx_transactions_store_id ON transactions (store_id);
CREATE INDEX IF NOT EXISTS idx_transactions_pos_transaction_id ON transactions (pos_transaction_id) WHERE pos_transaction_id IS NOT NULL;
-- Add composite index for common query patterns
CREATE INDEX IF NOT EXISTS idx_transactions_card_timestamp ON transactions(card_id, timestamp);

-- Use BRIN index for timestamp column (efficient for time-ordered data)
CREATE INDEX IF NOT EXISTS idx_transactions_timestamp_brin ON transactions USING BRIN (timestamp);

-- Create user_roles table (if it doesn't exist)
CREATE TABLE IF NOT EXISTS user_roles
(
    user_id UUID NOT NULL,
    role VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, role),
    CONSTRAINT fk_user_roles_users FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

-- Add proper indexing for user_roles table
CREATE INDEX IF NOT EXISTS idx_user_roles_role ON user_roles(role);


-- Create Password Reset Table
CREATE TABLE IF NOT EXISTS verification_tokens
(
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    token VARCHAR(256) NOT NULL,
    token_type VARCHAR(50) NOT NULL,
    is_valid BOOLEAN NOT NULL DEFAULT TRUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_verification_tokens_userid ON verification_tokens(user_id);
CREATE INDEX idx_verification_tokens_token ON verification_tokens(token);

-- Webhooks & Integrations
CREATE TABLE IF NOT EXISTS webhooks
(
    id UUID PRIMARY KEY,
    business_id UUID REFERENCES businesses(id),
    url VARCHAR(255) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    secret VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS webhook_deliveries
(
    id UUID PRIMARY KEY,
    webhook_id UUID REFERENCES webhooks(id),
    payload JSONB,
    status VARCHAR(50),
    response_code INT,
    response_body TEXT,
    attempted_at TIMESTAMP,
    retry_count INT DEFAULT 0
);

-- Notifications
CREATE TABLE IF NOT EXISTS notification_templates
(
    id UUID PRIMARY KEY,
    business_id UUID REFERENCES businesses(id),
    type VARCHAR(50),
    event_type VARCHAR(100),
    subject VARCHAR(255),
    body TEXT,
    language VARCHAR(10),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS notification_logs
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    type VARCHAR(50),
    event_type VARCHAR(100),
    sent_at TIMESTAMP,
    status VARCHAR(50),
    payload JSONB
);

CREATE TABLE IF NOT EXISTS notification_preferences
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    type VARCHAR(50),
    subscribed BOOLEAN NOT NULL DEFAULT TRUE,
    language VARCHAR(10),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Audit & API Usage
CREATE TABLE IF NOT EXISTS audit_logs
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    action VARCHAR(100),
    entity_type VARCHAR(100),
    entity_id UUID,
    details JSONB,
    performed_at TIMESTAMP,
    ip_address VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS api_clients
(
    id UUID PRIMARY KEY,
    business_id UUID REFERENCES businesses(id),
    name VARCHAR(100),
    api_key VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS api_usage_logs
(
    id UUID PRIMARY KEY,
    api_client_id UUID REFERENCES api_clients(id),
    endpoint VARCHAR(255),
    method VARCHAR(10),
    status_code INT,
    request_at TIMESTAMP,
    response_time_ms INT
);

-- Localization
CREATE TABLE IF NOT EXISTS locales 
(
    id UUID PRIMARY KEY,
    code VARCHAR(10) NOT NULL,
    name VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS translatable_strings
(
    id UUID PRIMARY KEY,
    key VARCHAR(100) NOT NULL,
    default_text VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS translations
(
    id UUID PRIMARY KEY,
    translatable_string_id UUID REFERENCES translatable_strings(id),
    locale_id UUID REFERENCES locales(id),
    text VARCHAR(255)
);

-- Privacy/Compliance
CREATE TABLE IF NOT EXISTS data_export_requests
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    requested_at TIMESTAMP NOT NULL,
    status VARCHAR(50),
    download_url VARCHAR(255),
    completed_at TIMESTAMP
);

CREATE TABLE IF NOT EXISTS account_deletion_requests
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    requested_at TIMESTAMP NOT NULL,
    status VARCHAR(50),
    completed_at TIMESTAMP
);

-- Reward Fulfillment
CREATE TABLE IF NOT EXISTS reward_redemptions
(
    id UUID PRIMARY KEY,
    reward_id UUID REFERENCES rewards(id),
    loyalty_card_id UUID REFERENCES loyalty_cards(id),
    customer_id UUID REFERENCES customers(id),
    status VARCHAR(50),
    requested_at TIMESTAMP,
    fulfilled_at TIMESTAMP,
    fulfillment_details JSONB,
    staff_id UUID REFERENCES users(id)
);

-- Support/Escalation
CREATE TABLE IF NOT EXISTS support_tickets
(
    id UUID PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    subject VARCHAR(255),
    description TEXT,
    status VARCHAR(50),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    assigned_to_staff_id UUID REFERENCES users(id)
);


-- MOVED INSIDE main DO block: Delete any existing admin users to avoid duplicates
DELETE FROM users WHERE username = 'admin';
-- We can keep the delete for manager for safety or remove if not needed
-- DELETE FROM users WHERE username = 'manager';

-- MOVED INSIDE main DO block: Insert admin user and role within a sub-DO block (or directly)
-- We can actually insert directly here without a nested DO block since we are already inside the main one
DECLARE
    admin_uuid UUID;
    staff_uuid UUID; 
    manager_uuid UUID;
BEGIN
    -- Generate UUID for the admin user
    admin_uuid := uuid_generate_v4();
    staff_uuid := uuid_generate_v4();
    manager_uuid := uuid_generate_v4();
    
    -- Insert admin user with the generated UUID and a hardcoded PrefixedId
    INSERT INTO users
    (
        id, prefixed_id, first_name, last_name, username, email, 
        password_hash, status, created_at, updated_at, email_confirmed
    )
    VALUES
    (
        admin_uuid, -- Use generated UUID
        'usr_t36klk2uczyedfc7ogufqb6111', -- Use hardcoded placeholder PrefixedId
        'admin',
        'admin',
        'admin',
        'admin@loyaltysystem.com',
        'AQAAAAIAAYagAAAAEMhipWjYGgWGXfBRQ3CIOi7X5sU2iAKpcZZi4rCmvnLtgzMTrHWvwuAe7LTdNCUqnA==',
        1, -- Active status
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        true
    ),
    (
        staff_uuid, -- Use generated UUID
        'usr_t36klk2uczyedfc7ogufqb6222', -- Use hardcoded placeholder PrefixedId
        'staff',
        'staff',
        'staff',
        'staff@loyaltysystem.com',
        'AQAAAAIAAYagAAAAEMhipWjYGgWGXfBRQ3CIOi7X5sU2iAKpcZZi4rCmvnLtgzMTrHWvwuAe7LTdNCUqnA==',
        1, -- Active status
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        true
    ),
    (
        manager_uuid, -- Use generated UUID
        'usr_t36klk2uczyedfc7ogufqb6333', -- Use hardcoded placeholder PrefixedId
        'manager',
        'manager',
        'manager',
        'manager@loyaltysystem.com',
        'AQAAAAIAAYagAAAAEMhipWjYGgWGXfBRQ3CIOi7X5sU2iAKpcZZi4rCmvnLtgzMTrHWvwuAe7LTdNCUqnA==',
        1, -- Active status
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        true
    );

    -- Add SuperAdmin role to the admin user using the generated UUID
    INSERT INTO user_roles (user_id, role)
    VALUES 
        (admin_uuid, 'User'),
        (staff_uuid, 'User'),
        (manager_uuid, 'User'),
        (staff_uuid, 'Staff'),
        (manager_uuid, 'Manager'),
        (admin_uuid, 'SuperAdmin');
END;
-- End of direct insertion logic (removed nested DO block)

-- ANALYTICS SUPPORT WITH MATERIALIZED VIEWS

-- Program metrics materialized view for fast reporting
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_program_metrics AS
SELECT
    p.id AS program_id,
    p.name AS program_name,
    b.id AS brand_id,
    b.name AS brand_name,
    COUNT(DISTINCT c.id) AS total_cards,
    COUNT(DISTINCT CASE WHEN c.status = 'Active' THEN c.id END) AS active_cards,
    SUM(CASE WHEN c.status = 'Active' THEN c.points_balance ELSE 0 END) AS total_active_points,
    COUNT(DISTINCT t.id) AS total_transactions,
    COALESCE(AVG(CASE WHEN t.type = 'PointsIssuance' THEN t.points_amount END), 0) AS avg_points_per_transaction,
    COALESCE(AVG(t.transaction_amount), 0) AS avg_transaction_amount
FROM loyalty_programs p
     JOIN brands b ON p.brand_id = b.id
     LEFT JOIN loyalty_cards c ON p.id = c.program_id
     LEFT JOIN transactions t ON c.id = t.card_id
GROUP BY p.id, p.name, b.id, b.name
    WITH DATA;

CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_program_metrics_program_id ON mv_program_metrics(program_id);
CREATE INDEX IF NOT EXISTS idx_mv_program_metrics_brand_id ON mv_program_metrics(brand_id);

-- Customer engagement metrics
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_customer_metrics AS
SELECT
    c.id AS customer_id,
    c.first_name,
    c.last_name,
    COUNT(DISTINCT lc.id) AS total_cards,
    SUM(CASE WHEN lc.status = 'Active' THEN 1 ELSE 0 END) AS active_cards,
    SUM(lc.points_balance) AS total_points,
    MAX(t.timestamp) AS last_transaction_date,
    COUNT(DISTINCT t.id) AS total_transactions,
    COUNT(DISTINCT CASE WHEN t.type = 'RewardRedemption' THEN t.id END) AS total_redemptions
FROM customers c
         LEFT JOIN loyalty_cards lc ON c.id = lc.customer_id
         LEFT JOIN transactions t ON lc.id = t.card_id
GROUP BY c.id, c.first_name, c.last_name
    WITH DATA;

CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_customer_metrics_customer_id ON mv_customer_metrics(customer_id);

-- SECURITY WITH ROW-LEVEL SECURITY

-- Create an app context for current brand/store/staff access control
CREATE TABLE IF NOT EXISTS app_context 
(
   key TEXT PRIMARY KEY,
   value TEXT NOT NULL
);

-- Enable row-level security on tables
ALTER TABLE brands ENABLE ROW LEVEL SECURITY;
ALTER TABLE stores ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_programs ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE transactions ENABLE ROW LEVEL SECURITY;
ALTER TABLE rewards ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for multi-tenant isolation
-- Brand isolation
DROP POLICY IF EXISTS brand_isolation ON brands;
    CREATE POLICY brand_isolation ON brands
        USING (id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

    -- Store isolation by brand
    DROP POLICY IF EXISTS store_isolation ON stores;
    CREATE POLICY store_isolation ON stores
        USING (brand_id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

    -- Program isolation by brand
    DROP POLICY IF EXISTS program_isolation ON loyalty_programs;
    CREATE POLICY program_isolation ON loyalty_programs
        USING (brand_id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

    -- Card isolation by brand (through program)
    DROP POLICY IF EXISTS card_isolation ON loyalty_cards;
    CREATE POLICY card_isolation ON loyalty_cards
        USING (program_id IN (
            SELECT id FROM loyalty_programs 
            WHERE brand_id::TEXT = get_app_context('current_brand_id')
        ) OR get_app_context('user_role') = 'admin');

    -- Transaction isolation by store
    DROP POLICY IF EXISTS transaction_isolation ON transactions;
    CREATE POLICY transaction_isolation ON transactions
        USING (store_id::TEXT = get_app_context('current_store_id') 
            OR store_id IN (
                SELECT id FROM stores 
                WHERE brand_id::TEXT = get_app_context('current_brand_id')
            )
            OR get_app_context('user_role') = 'admin');

    -- Reward isolation by brand (through program)
    DROP POLICY IF EXISTS reward_isolation ON rewards;
    CREATE POLICY reward_isolation ON rewards
        USING (program_id IN (
            SELECT id FROM loyalty_programs 
            WHERE brand_id::TEXT = get_app_context('current_brand_id')
        ) OR get_app_context('user_role') = 'admin');

    -- Output success message
    RAISE NOTICE 'Database setup completed successfully with the unified users table!';

    -- Create indexes for the business table
CREATE INDEX IF NOT EXISTS idx_businesses_name ON businesses (name);
CREATE INDEX IF NOT EXISTS idx_businesses_is_active ON businesses (is_active);
END;
$$;

-- MOVED HERE: Insert the base tiers AFTER tables are created
DO $$
DECLARE
program_record RECORD;
BEGIN
FOR program_record IN SELECT id FROM loyalty_programs WHERE type = 'Points' LOOP -- Correct comparison using ENUM literal
  -- Insert a basic tier (necessary for compatibility)
  -- Use ON CONFLICT to avoid errors if the script is run multiple times or tier already exists
  INSERT INTO loyalty_program_tiers
  (
        id,
        program_id,
        name,
        point_threshold,
        point_multiplier,
        tier_order
    )
    VALUES
    (
        uuid_generate_v4(), -- Use uuid_generate_v4() for consistency
        program_record.id,
        'Base Tier',
        0,
        1.0,
        1
    )
    ON CONFLICT (program_id, tier_order) DO NOTHING; -- Prevent errors if tier exists
END LOOP;
END $$;

-- Apply triggers after tables exist


DO $$
BEGIN
    -- Apply timestamp update triggers to all tables that need it
DROP TRIGGER IF EXISTS update_businesses_timestamp ON businesses;
CREATE TRIGGER update_businesses_timestamp
    BEFORE UPDATE ON businesses
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_business_contacts_timestamp ON business_contacts;
CREATE TRIGGER update_business_contacts_timestamp
    BEFORE UPDATE ON business_contacts
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_business_addresses_timestamp ON business_addresses;
CREATE TRIGGER update_business_addresses_timestamp
    BEFORE UPDATE ON business_addresses
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_brands_timestamp ON brands;
CREATE TRIGGER update_brands_timestamp
    BEFORE UPDATE ON brands
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_stores_timestamp ON stores;
CREATE TRIGGER update_stores_timestamp
    BEFORE UPDATE ON stores
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_store_contacts_timestamp ON store_contacts;
CREATE TRIGGER update_store_contacts_timestamp
    BEFORE UPDATE ON store_contacts
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_loyalty_programs_timestamp ON loyalty_programs;
CREATE TRIGGER update_loyalty_programs_timestamp
    BEFORE UPDATE ON loyalty_programs
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_rewards_timestamp ON rewards;
CREATE TRIGGER update_rewards_timestamp
    BEFORE UPDATE ON rewards
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_loyalty_cards_timestamp ON loyalty_cards;
CREATE TRIGGER update_loyalty_cards_timestamp
    BEFORE UPDATE ON loyalty_cards
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_card_links_timestamp ON card_links;
CREATE TRIGGER update_card_links_timestamp
    BEFORE UPDATE ON card_links
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

DROP TRIGGER IF EXISTS update_users_timestamp ON users;
CREATE TRIGGER update_users_timestamp
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();
END;
$$;

-- Apply Business entity specific RLS policies
DO $$
BEGIN
    -- Enable row-level security on business tables
ALTER TABLE businesses ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for business isolation
DROP POLICY IF EXISTS business_isolation ON businesses;
    CREATE POLICY business_isolation ON businesses
        USING (id::TEXT = get_app_context('current_business_id') OR get_app_context('user_role') = 'SuperAdmin');
    
    -- Update Brand isolation to include business context
    DROP POLICY IF EXISTS brand_isolation ON brands;
    CREATE POLICY brand_isolation ON brands
        USING (
            id::TEXT = get_app_context('current_brand_id') OR 
            business_id::TEXT = get_app_context('current_business_id') OR 
            get_app_context('user_role') = 'SuperAdmin'
        );
END;
$$;

COMMIT; 