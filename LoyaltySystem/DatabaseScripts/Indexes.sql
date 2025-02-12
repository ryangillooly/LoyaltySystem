-- =========================================
-- 1. Users
-- =========================================
-- If you want a unique index on Email (assuming each email must be unique):
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email
    ON users (email);

-- =========================================
-- 2. BusinessUser (FK: BusinessId, UserId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_businessusers_businessid
    ON businessusers (businessid);

CREATE INDEX IF NOT EXISTS idx_businessusers_userid
    ON businessusers (userid);

-- =========================================
-- 3. Store (FK: BusinessId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_stores_businessid
    ON stores (businessid);

-- =========================================
-- 4. FraudSettings (FK: StoreId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_fraudsettings_storeid
    ON fraudsettings (storeid);

-- =========================================
-- 5. QRCodeDesign (FK: BusinessId, StoreId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_qrcodedesigns_businessid
    ON qrcodedesigns (businessid);

CREATE INDEX IF NOT EXISTS idx_qrcodedesigns_storeid
    ON qrcodedesigns (storeid);

-- =========================================
-- 6. LoyaltyCardTemplate (FK: BusinessId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_loyaltycardtemplates_businessid
    ON loyaltycardtemplates (businessid);

-- =========================================
-- 7. UserLoyaltyCard (FK: UserId, LoyaltyCardTemplateId, BusinessId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_userloyaltycards_userid
    ON userloyaltycards (userid);

CREATE INDEX IF NOT EXISTS idx_userloyaltycards_templateid
    ON userloyaltycards (loyaltycardtemplateid);

CREATE INDEX IF NOT EXISTS idx_userloyaltycards_businessid
    ON userloyaltycards (businessid);

-- If you often query by (UserId, Status):
-- CREATE INDEX IF NOT EXISTS idx_userloyaltycard_userid_status
-- ON userloyaltycard (userid, status);

-- =========================================
-- 8. StampTransaction (FK: UserLoyaltyCardId, StoreId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_stamptransactions_userloyaltycardid
    ON stamptransactions (userloyaltycardid);

CREATE INDEX IF NOT EXISTS idx_stamptransactions_storeid
    ON stamptransactions (storeid);

-- If you do frequent date-range queries on Timestamp:
CREATE INDEX IF NOT EXISTS idx_stamptransactions_timestamp
    ON stamptransactions (timestamp);

-- Composite if you regularly filter on business + timestamp:
-- CREATE INDEX IF NOT EXISTS idx_stamptransaction_businessid_timestamp
-- ON stamptransaction (businessid, timestamp);

-- =========================================
-- 9. Reward (FK: BusinessId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_rewards_businessid
    ON rewards (businessid);

-- =========================================
-- 10. LoyaltyProgramSettings (FK: BusinessId, BirthdayRewardId, WelcomeRewardId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_loyaltyprogramsettings_businessid
    ON loyaltyprogramsettings (businessid);

CREATE INDEX IF NOT EXISTS idx_loyaltyprogramsettings_birthdayrewardid
    ON loyaltyprogramsettings (birthdayrewardid);

CREATE INDEX IF NOT EXISTS idx_loyaltyprogramsettings_welcomerewardid
    ON loyaltyprogramsettings (welcomerewardid);

-- =========================================
-- 11. LoyaltyCardRewardMapping (FK: LoyaltyCardTemplateId, RewardId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_loyaltycardrewardmappings_templateid
    ON loyaltycardrewardmappings (loyaltycardtemplateid);

CREATE INDEX IF NOT EXISTS idx_loyaltycardrewardmappings_rewardid
    ON loyaltycardrewardmappings (rewardid);

-- =========================================
-- 12. RedemptionTransaction (FK: UserLoyaltyCardId, RewardId, StoreId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_redemptiontransactions_userloyaltycardid
    ON redemptiontransactions (userloyaltycardid);

CREATE INDEX IF NOT EXISTS idx_redemptiontransactions_rewardid
    ON redemptiontransactions (rewardid);

CREATE INDEX IF NOT EXISTS idx_redemptiontransaction_storeid
    ON redemptiontransactions (storeid);

-- If you do date-range queries on redemption timestamps:
CREATE INDEX IF NOT EXISTS idx_redemptiontransactions_timestamp
    ON redemptiontransactions (timestamp);

-- =========================================
-- 13. Promotion (FK: BusinessId)
-- =========================================
CREATE INDEX IF NOT EXISTS idx_promotion_businessid
    ON promotions (businessid);

-- If you filter by ValidFrom / ValidUntil:
-- CREATE INDEX IF NOT EXISTS idx_promotion_validfrom_validuntil
-- ON promotion (validfrom, validuntil);

-- End of Index Script
