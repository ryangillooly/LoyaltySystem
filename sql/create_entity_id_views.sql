-- SQL views to help visualize both raw UUIDs and prefixed entity IDs
-- Use for development and debugging purposes

-- Customers view with prefixed IDs
CREATE OR REPLACE VIEW vw_customers_with_ids AS
SELECT 
    id as raw_id,
    concat('cus_', replace(id::text, '-', '')) as prefixed_id,
    name,
    email,
    phone,
    created_at,
    updated_at
FROM customers;

-- Users view with prefixed IDs
CREATE OR REPLACE VIEW vw_users_with_ids AS
SELECT 
    id as raw_id,
    concat('usr_', replace(id::text, '-', '')) as prefixed_id,
    username,
    email,
    customer_id as raw_customer_id,
    CASE 
        WHEN customer_id IS NOT NULL THEN concat('cus_', replace(customer_id::text, '-', ''))
        ELSE NULL
    END as prefixed_customer_id,
    status,
    created_at,
    updated_at,
    last_login_at
FROM users;

-- Loyalty Programs view with prefixed IDs
CREATE OR REPLACE VIEW vw_loyalty_programs_with_ids AS
SELECT 
    id as raw_id,
    concat('lprog_', replace(id::text, '-', '')) as prefixed_id,
    name,
    brand_id as raw_brand_id,
    concat('brd_', replace(brand_id::text, '-', '')) as prefixed_brand_id,
    description,
    type,
    created_at,
    updated_at
FROM loyalty_programs;

-- Loyalty Cards view with prefixed IDs
CREATE OR REPLACE VIEW vw_loyalty_cards_with_ids AS
SELECT 
    c.id as raw_id,
    concat('card_', replace(c.id::text, '-', '')) as prefixed_id,
    c.program_id as raw_program_id,
    concat('lprog_', replace(c.program_id::text, '-', '')) as prefixed_program_id,
    c.customer_id as raw_customer_id,
    concat('cus_', replace(c.customer_id::text, '-', '')) as prefixed_customer_id,
    c.type,
    c.stamps_collected,
    c.points_balance,
    c.status,
    c.created_at,
    c.updated_at,
    c.qr_code,
    p.name as program_name,
    cu.name as customer_name
FROM loyalty_cards c
LEFT JOIN loyalty_programs p ON c.program_id = p.id
LEFT JOIN customers cu ON c.customer_id = cu.id;

-- Transactions view with prefixed IDs
CREATE OR REPLACE VIEW vw_transactions_with_ids AS
SELECT 
    t.id as raw_id,
    concat('tx_', replace(t.id::text, '-', '')) as prefixed_id,
    t.card_id as raw_card_id,
    concat('card_', replace(t.card_id::text, '-', '')) as prefixed_card_id,
    t.type,
    t.reward_id as raw_reward_id,
    CASE 
        WHEN t.reward_id IS NOT NULL THEN concat('rwd_', replace(t.reward_id::text, '-', ''))
        ELSE NULL
    END as prefixed_reward_id,
    t.quantity,
    t.points_amount,
    t.transaction_amount,
    t.store_id as raw_store_id,
    CASE 
        WHEN t.store_id IS NOT NULL THEN concat('str_', replace(t.store_id::text, '-', ''))
        ELSE NULL
    END as prefixed_store_id,
    t.staff_id as raw_staff_id,
    CASE 
        WHEN t.staff_id IS NOT NULL THEN concat('usr_', replace(t.staff_id::text, '-', ''))
        ELSE NULL
    END as prefixed_staff_id,
    t.pos_transaction_id,
    t.timestamp,
    t.created_at,
    t.metadata
FROM transactions t;

-- User Roles view with prefixed IDs
CREATE OR REPLACE VIEW vw_user_roles_with_ids AS
SELECT 
    ur.user_id as raw_user_id,
    concat('usr_', replace(ur.user_id::text, '-', '')) as prefixed_user_id,
    ur.role,
    ur.created_at,
    u.username
FROM user_roles ur
JOIN users u ON ur.user_id = u.id;

-- Example usage:
-- SELECT * FROM vw_customers_with_ids;
-- SELECT * FROM vw_users_with_ids;
-- SELECT * FROM vw_loyalty_programs_with_ids;
-- SELECT * FROM vw_loyalty_cards_with_ids;
-- SELECT * FROM vw_transactions_with_ids;
-- SELECT * FROM vw_user_roles_with_ids;

-- Example: Find a customer by their prefixed ID
-- SELECT * FROM vw_customers_with_ids WHERE prefixed_id = 'cus_123456789abcdef123456789';

-- Example: Find all transactions for a specific card
-- SELECT * FROM vw_transactions_with_ids WHERE prefixed_card_id = 'card_123456789abcdef123456789';

-- Example: Find a user with their assigned roles
-- SELECT u.*, r.role 
-- FROM vw_users_with_ids u
-- JOIN vw_user_roles_with_ids r ON u.raw_id = r.raw_user_id
-- WHERE u.prefixed_id = 'usr_123456789abcdef123456789'; 