-- Migration: Add Loyalty Tiers and Points Configuration

-- Add new columns to loyalty_programs table
ALTER TABLE loyalty_programs
    ADD COLUMN has_tiers BOOLEAN NOT NULL DEFAULT FALSE,
    ADD COLUMN points_per_dollar DECIMAL(10, 2) NULL,
    ADD COLUMN minimum_points_for_redemption INTEGER NULL,
    ADD COLUMN points_rounding_rule SMALLINT NULL,
    ADD COLUMN enrollment_bonus_points INTEGER NULL;

-- Create loyalty program tiers table
CREATE TABLE loyalty_program_tiers (
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

-- Create index for faster lookups
CREATE INDEX idx_loyalty_program_tiers_program_id ON loyalty_program_tiers(program_id);

-- Create loyalty tier benefits table
CREATE TABLE loyalty_tier_benefits (
    id UUID PRIMARY KEY,
    tier_id UUID NOT NULL REFERENCES loyalty_program_tiers(id) ON DELETE CASCADE,
    benefit_description TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create index for faster lookups
CREATE INDEX idx_loyalty_tier_benefits_tier_id ON loyalty_tier_benefits(tier_id);

-- Insert the base tiers for existing points-based loyalty programs
DO $$
DECLARE
    program_record RECORD;
BEGIN
    FOR program_record IN SELECT id FROM loyalty_programs WHERE type = 1 LOOP -- Type 1 is Points
        -- Insert a basic tier (necessary for compatibility)
        INSERT INTO loyalty_program_tiers (
            id,
            program_id,
            name,
            point_threshold,
            point_multiplier,
            tier_order
        ) VALUES (
            gen_random_uuid(),
            program_record.id,
            'Base Tier',
            0,
            1.0,
            1
        );
    END LOOP;
END $$; 