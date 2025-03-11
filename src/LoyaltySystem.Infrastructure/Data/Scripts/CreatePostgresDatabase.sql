-- Create custom types using PostgreSQL ENUM
CREATE TYPE loyalty_program_type AS ENUM ('Stamp', 'Points');
CREATE TYPE transaction_type AS ENUM ('StampIssuance', 'PointsIssuance', 'RewardRedemption', 'StampVoid', 'PointsVoid');
CREATE TYPE card_status AS ENUM ('Active', 'Expired', 'Suspended');
CREATE TYPE expiration_type AS ENUM ('Days', 'Months', 'Years');
CREATE TYPE card_link_type AS ENUM ('PaymentCard', 'PhoneNumber', 'Email', 'Other');

-- Create extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create extension for geospatial functionality
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create extension for text search and similarity
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create Brands table
CREATE TABLE IF NOT EXISTS brands (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    category VARCHAR(50) NULL,
    logo VARCHAR(255) NULL,
    description VARCHAR(500) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create BrandContacts table
CREATE TABLE IF NOT EXISTS brand_contacts (
    brand_id UUID PRIMARY KEY,
    email VARCHAR(100) NULL,
    phone VARCHAR(50) NULL,
    website VARCHAR(255) NULL,
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
    CONSTRAINT fk_brand_addresses_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
);

-- Create Stores table
CREATE TABLE IF NOT EXISTS stores (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    brand_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    contact_info VARCHAR(255) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_stores_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE
);

CREATE INDEX idx_stores_brand_id ON stores (brand_id);

-- Create StoreAddresses table
CREATE TABLE IF NOT EXISTS store_addresses (
    store_id UUID PRIMARY KEY,
    line1 VARCHAR(100) NOT NULL,
    line2 VARCHAR(100) NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NULL,
    postal_code VARCHAR(20) NULL,
    country VARCHAR(50) NOT NULL,
    CONSTRAINT fk_store_addresses_stores FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
);

-- Create StoreGeoLocations table using PostGIS
CREATE TABLE IF NOT EXISTS store_geo_locations (
    store_id UUID PRIMARY KEY,
    location GEOGRAPHY(POINT) NOT NULL,
    CONSTRAINT fk_store_geo_locations_stores FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
);

-- Create spatial index
CREATE INDEX idx_store_geo_locations_location ON store_geo_locations USING GIST (location);

-- Create StoreOperatingHours table
CREATE TABLE IF NOT EXISTS store_operating_hours (
    store_id UUID NOT NULL,
    day_of_week INTEGER NOT NULL,
    open_time TIME NOT NULL,
    close_time TIME NOT NULL,
    CONSTRAINT pk_store_operating_hours PRIMARY KEY (store_id, day_of_week),
    CONSTRAINT fk_store_operating_hours_stores FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE,
    CONSTRAINT chk_day_of_week CHECK (day_of_week BETWEEN 0 AND 6)
);

-- Create Customers table
CREATE TABLE IF NOT EXISTS customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NULL,
    phone VARCHAR(50) NULL,
    marketing_consent BOOLEAN NOT NULL DEFAULT FALSE,
    joined_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_customers_email ON customers (email) WHERE email IS NOT NULL;
CREATE INDEX idx_customers_phone ON customers (phone) WHERE phone IS NOT NULL;
-- Add enhanced search for customers by name (using trigram similarity)
CREATE INDEX idx_customers_name_gin ON customers USING gin(name gin_trgm_ops);

-- Create LoyaltyPrograms table
CREATE TABLE IF NOT EXISTS loyalty_programs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    brand_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    type loyalty_program_type NOT NULL,
    stamp_threshold INTEGER NULL,
    points_conversion_rate NUMERIC(18, 2) NULL,
    daily_stamp_limit INTEGER NULL,
    minimum_transaction_amount NUMERIC(18, 2) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_loyalty_programs_brands FOREIGN KEY (brand_id) REFERENCES brands (id) ON DELETE CASCADE,
    CONSTRAINT chk_stamp_threshold CHECK (type != 'Stamp' OR stamp_threshold IS NOT NULL),
    CONSTRAINT chk_points_conversion CHECK (type != 'Points' OR points_conversion_rate IS NOT NULL)
);

CREATE INDEX idx_loyalty_programs_brand_id ON loyalty_programs (brand_id);
CREATE INDEX idx_loyalty_programs_type ON loyalty_programs (type);

-- Create ProgramExpirationPolicies table
CREATE TABLE IF NOT EXISTS program_expiration_policies (
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
CREATE TABLE IF NOT EXISTS rewards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
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

CREATE INDEX idx_rewards_program_id ON rewards (program_id);
CREATE INDEX idx_rewards_valid_period ON rewards (valid_from, valid_to) WHERE valid_from IS NOT NULL AND valid_to IS NOT NULL;
-- Add index for active rewards (commonly queried)
CREATE INDEX idx_rewards_program_active ON rewards(program_id, is_active) WHERE is_active = TRUE;

-- Create LoyaltyCards table
CREATE TABLE IF NOT EXISTS loyalty_cards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    program_id UUID NOT NULL,
    customer_id UUID NOT NULL,
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

CREATE UNIQUE INDEX idx_loyalty_cards_qr_code ON loyalty_cards (qr_code);
CREATE INDEX idx_loyalty_cards_program_id ON loyalty_cards (program_id);
CREATE INDEX idx_loyalty_cards_customer_id ON loyalty_cards (customer_id);
CREATE INDEX idx_loyalty_cards_status ON loyalty_cards (status);
CREATE INDEX idx_loyalty_cards_expires_at ON loyalty_cards (expires_at) WHERE expires_at IS NOT NULL;
-- Add composite index for common query patterns
CREATE INDEX idx_loyalty_cards_program_status ON loyalty_cards(program_id, status);

-- Create Transactions table with partitioning
CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    card_id UUID NOT NULL,
    type transaction_type NOT NULL,
    reward_id UUID NULL,
    quantity INTEGER NULL,
    points_amount NUMERIC(18, 2) NULL,
    transaction_amount NUMERIC(18, 2) NULL,
    store_id UUID NOT NULL,
    staff_id UUID NULL,
    pos_transaction_id VARCHAR(100) NULL,
    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB NULL,
    CONSTRAINT fk_transactions_loyalty_cards FOREIGN KEY (card_id) REFERENCES loyalty_cards (id),
    CONSTRAINT fk_transactions_rewards FOREIGN KEY (reward_id) REFERENCES rewards (id),
    CONSTRAINT fk_transactions_stores FOREIGN KEY (store_id) REFERENCES stores (id),
    CONSTRAINT chk_reward_redemption CHECK (type != 'RewardRedemption' OR reward_id IS NOT NULL),
    CONSTRAINT chk_quantity CHECK (type != 'StampIssuance' OR quantity IS NOT NULL),
    CONSTRAINT chk_points CHECK (type != 'PointsIssuance' OR points_amount IS NOT NULL)
) PARTITION BY RANGE (timestamp);

-- Create partitions for transactions (past year and current year)
CREATE TABLE transactions_2023 PARTITION OF transactions
    FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');
CREATE TABLE transactions_2024 PARTITION OF transactions
    FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
-- Add partition for next year
CREATE TABLE transactions_2025 PARTITION OF transactions
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

-- Create indexes for transactions
CREATE INDEX idx_transactions_card_id ON transactions (card_id);
CREATE INDEX idx_transactions_type ON transactions (type);
CREATE INDEX idx_transactions_store_id ON transactions (store_id);
CREATE INDEX idx_transactions_pos_transaction_id ON transactions (pos_transaction_id) WHERE pos_transaction_id IS NOT NULL;
-- Add composite index for common query patterns
CREATE INDEX idx_transactions_card_timestamp ON transactions(card_id, timestamp);

-- Use BRIN index for timestamp column (efficient for time-ordered data)
CREATE INDEX idx_transactions_timestamp_brin ON transactions USING BRIN (timestamp);

-- Create CardLinks table
CREATE TABLE IF NOT EXISTS card_links (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    card_id UUID NOT NULL,
    card_hash VARCHAR(255) NOT NULL,
    link_type card_link_type NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_card_links_loyalty_cards FOREIGN KEY (card_id) REFERENCES loyalty_cards (id)
);

CREATE UNIQUE INDEX idx_card_links_card_hash ON card_links (card_hash);
CREATE INDEX idx_card_links_card_id ON card_links (card_id);

-- PERFORMANCE OPTIMIZATIONS FOR HIGH-VOLUME TABLES

-- Optimize autovacuum for transactions table
ALTER TABLE transactions SET (
    autovacuum_vacuum_scale_factor = 0.05,
    autovacuum_analyze_scale_factor = 0.025,
    fillfactor = 80
);

-- Optimize loyalty_cards table
ALTER TABLE loyalty_cards SET (
    autovacuum_vacuum_scale_factor = 0.1,
    autovacuum_analyze_scale_factor = 0.05,
    fillfactor = 90
);

-- PARTITION MANAGEMENT TOOLS

-- Create table to track partition maintenance
CREATE TABLE IF NOT EXISTS partition_maintenance_log (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    year INTEGER NOT NULL,
    notes TEXT
);

-- Procedure to create new transaction partitions automatically
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

-- Create functions for automatic timestamp updates
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply timestamp update triggers to all tables that need it
CREATE TRIGGER update_brands_timestamp
BEFORE UPDATE ON brands
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_stores_timestamp
BEFORE UPDATE ON stores
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_loyalty_programs_timestamp
BEFORE UPDATE ON loyalty_programs
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_rewards_timestamp
BEFORE UPDATE ON rewards
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_loyalty_cards_timestamp
BEFORE UPDATE ON loyalty_cards
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_card_links_timestamp
BEFORE UPDATE ON card_links
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

-- DATA INTEGRITY ENHANCEMENTS

-- Ensure card type matches program type
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

CREATE TRIGGER tr_check_card_program_type_consistency
BEFORE INSERT OR UPDATE ON loyalty_cards
FOR EACH ROW
EXECUTE FUNCTION check_card_program_type_consistency();

-- Validate reward redemption
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

CREATE TRIGGER tr_validate_reward_redemption
BEFORE INSERT ON transactions
FOR EACH ROW
EXECUTE FUNCTION validate_reward_redemption();

-- ANALYTICS SUPPORT WITH MATERIALIZED VIEWS

-- Program metrics materialized view for fast reporting
CREATE MATERIALIZED VIEW mv_program_metrics AS
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

CREATE UNIQUE INDEX idx_mv_program_metrics_program_id ON mv_program_metrics(program_id);
CREATE INDEX idx_mv_program_metrics_brand_id ON mv_program_metrics(brand_id);

-- Customer engagement metrics
CREATE MATERIALIZED VIEW mv_customer_metrics AS
SELECT 
    c.id AS customer_id,
    c.name AS customer_name,
    COUNT(DISTINCT lc.id) AS total_cards,
    SUM(CASE WHEN lc.status = 'Active' THEN 1 ELSE 0 END) AS active_cards,
    SUM(lc.points_balance) AS total_points,
    MAX(t.timestamp) AS last_transaction_date,
    COUNT(DISTINCT t.id) AS total_transactions,
    COUNT(DISTINCT CASE WHEN t.type = 'RewardRedemption' THEN t.id END) AS total_redemptions
FROM customers c
LEFT JOIN loyalty_cards lc ON c.id = lc.customer_id
LEFT JOIN transactions t ON lc.id = t.card_id
GROUP BY c.id, c.name
WITH DATA;

CREATE UNIQUE INDEX idx_mv_customer_metrics_customer_id ON mv_customer_metrics(customer_id);

-- Function to refresh materialized views
CREATE OR REPLACE PROCEDURE refresh_analytics_views()
LANGUAGE plpgsql
AS $$
BEGIN
    REFRESH MATERIALIZED VIEW mv_program_metrics;
    REFRESH MATERIALIZED VIEW mv_customer_metrics;
END;
$$;

-- SECURITY WITH ROW-LEVEL SECURITY

-- Create an app context for current brand/store/staff access control
CREATE TABLE IF NOT EXISTS app_context (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

-- Function to set app context
CREATE OR REPLACE FUNCTION set_app_context(p_key TEXT, p_value TEXT)
RETURNS VOID AS $$
BEGIN
    -- Delete existing key if present
    DELETE FROM app_context WHERE key = p_key;
    
    -- Insert new key-value
    INSERT INTO app_context (key, value) VALUES (p_key, p_value);
END;
$$ LANGUAGE plpgsql;

-- Function to get app context
CREATE OR REPLACE FUNCTION get_app_context(p_key TEXT)
RETURNS TEXT AS $$
DECLARE
    v_value TEXT;
BEGIN
    SELECT value INTO v_value FROM app_context WHERE key = p_key;
    RETURN v_value;
END;
$$ LANGUAGE plpgsql;

-- Enable row-level security on tables
ALTER TABLE brands ENABLE ROW LEVEL SECURITY;
ALTER TABLE stores ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_programs ENABLE ROW LEVEL SECURITY;
ALTER TABLE loyalty_cards ENABLE ROW LEVEL SECURITY;
ALTER TABLE transactions ENABLE ROW LEVEL SECURITY;
ALTER TABLE rewards ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for multi-tenant isolation
-- Brand isolation
CREATE POLICY brand_isolation ON brands
    USING (id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

-- Store isolation by brand
CREATE POLICY store_isolation ON stores
    USING (brand_id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

-- Program isolation by brand
CREATE POLICY program_isolation ON loyalty_programs
    USING (brand_id::TEXT = get_app_context('current_brand_id') OR get_app_context('user_role') = 'admin');

-- Card isolation by brand (through program)
CREATE POLICY card_isolation ON loyalty_cards
    USING (program_id IN (
        SELECT id FROM loyalty_programs 
        WHERE brand_id::TEXT = get_app_context('current_brand_id')
    ) OR get_app_context('user_role') = 'admin');

-- Transaction isolation by store
CREATE POLICY transaction_isolation ON transactions
    USING (store_id::TEXT = get_app_context('current_store_id') 
        OR store_id IN (
            SELECT id FROM stores 
            WHERE brand_id::TEXT = get_app_context('current_brand_id')
        )
        OR get_app_context('user_role') = 'admin');

-- Reward isolation by brand (through program)
CREATE POLICY reward_isolation ON rewards
    USING (program_id IN (
        SELECT id FROM loyalty_programs 
        WHERE brand_id::TEXT = get_app_context('current_brand_id')
    ) OR get_app_context('user_role') = 'admin');

-- Done
COMMENT ON DATABASE CURRENT_CATALOG IS 'Loyalty System Database - Created with PostgreSQL-specific optimizations'; 