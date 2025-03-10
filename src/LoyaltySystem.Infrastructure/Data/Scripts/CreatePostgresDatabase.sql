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

-- Create indexes for transactions
CREATE INDEX idx_transactions_card_id ON transactions (card_id);
CREATE INDEX idx_transactions_type ON transactions (type);
CREATE INDEX idx_transactions_store_id ON transactions (store_id);
CREATE INDEX idx_transactions_pos_transaction_id ON transactions (pos_transaction_id) WHERE pos_transaction_id IS NOT NULL;

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

-- Create functions for automatic timestamp updates
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create triggers for automatic timestamp updates
CREATE TRIGGER update_brands_timestamp
BEFORE UPDATE ON brands
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_stores_timestamp
BEFORE UPDATE ON stores
FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_customers_timestamp
BEFORE UPDATE ON customers
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

-- Create function to find nearby stores
CREATE OR REPLACE FUNCTION find_nearby_stores(
    lat DOUBLE PRECISION,
    lng DOUBLE PRECISION,
    radius_in_meters INTEGER DEFAULT 5000
)
RETURNS TABLE (
    id UUID,
    brand_id UUID,
    name VARCHAR,
    distance_in_meters DOUBLE PRECISION
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        s.id,
        s.brand_id,
        s.name,
        ST_Distance(
            sgl.location::geography,
            ST_SetSRID(ST_MakePoint(lng, lat), 4326)::geography
        ) AS distance_in_meters
    FROM 
        stores s
    JOIN 
        store_geo_locations sgl ON s.id = sgl.store_id
    WHERE 
        ST_DWithin(
            sgl.location::geography,
            ST_SetSRID(ST_MakePoint(lng, lat), 4326)::geography,
            radius_in_meters
        )
    ORDER BY 
        distance_in_meters;
END;
$$ LANGUAGE plpgsql;

-- Create function to expire loyalty cards
CREATE OR REPLACE FUNCTION expire_loyalty_cards()
RETURNS INTEGER AS $$
DECLARE
    expired_count INTEGER;
BEGIN
    UPDATE loyalty_cards
    SET 
        status = 'Expired',
        updated_at = CURRENT_TIMESTAMP
    WHERE 
        status = 'Active' AND
        expires_at IS NOT NULL AND
        expires_at < CURRENT_TIMESTAMP;
    
    GET DIAGNOSTICS expired_count = ROW_COUNT;
    RETURN expired_count;
END;
$$ LANGUAGE plpgsql;

-- Create a materialized view for analytics (active cards by program)
CREATE MATERIALIZED VIEW IF NOT EXISTS active_cards_by_program AS
SELECT 
    lp.id AS program_id,
    lp.name AS program_name,
    b.id AS brand_id,
    b.name AS brand_name,
    COUNT(*) AS active_card_count,
    SUM(CASE WHEN lc.type = 'Points' THEN lc.points_balance ELSE 0 END) AS total_points,
    SUM(CASE WHEN lc.type = 'Stamp' THEN lc.stamps_collected ELSE 0 END) AS total_stamps
FROM 
    loyalty_programs lp
JOIN 
    brands b ON lp.brand_id = b.id
JOIN 
    loyalty_cards lc ON lp.id = lc.program_id
WHERE 
    lc.status = 'Active'
GROUP BY 
    lp.id, lp.name, b.id, b.name
WITH DATA;

CREATE UNIQUE INDEX ON active_cards_by_program (program_id);

-- Create function to refresh the materialized view
CREATE OR REPLACE FUNCTION refresh_active_cards_view()
RETURNS VOID AS $$
BEGIN
    REFRESH MATERIALIZED VIEW active_cards_by_program;
END;
$$ LANGUAGE plpgsql;

-- Create a comment to explain how to schedule the refresh
COMMENT ON FUNCTION refresh_active_cards_view() IS 'Schedule this function to run daily using pg_cron extension or an external scheduler.';

-- Optionally add sample data
-- INSERT INTO brands (id, name, category, created_at, updated_at)
-- VALUES (uuid_generate_v4(), 'Sample Coffee Shop', 'Food & Beverage', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Done
COMMENT ON DATABASE CURRENT_CATALOG IS 'Loyalty System Database - Created with PostgreSQL-specific optimizations'; 