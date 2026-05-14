-- 2026-01-29 matching + outfits + lockout

-- Garment features storage
ALTER TABLE eitan_project12.garments
    ADD COLUMN IF NOT EXISTS feature_json LONGTEXT NULL,
    ADD COLUMN IF NOT EXISTS updated_at DATETIME NULL;

-- Outfit expansion to store user + garment composition + score/rank
ALTER TABLE eitan_project12.outfits
    ADD COLUMN IF NOT EXISTS user_id CHAR(36) NULL AFTER outfit_id,
    ADD COLUMN IF NOT EXISTS shirt_garment_id CHAR(36) NULL AFTER user_id,
    ADD COLUMN IF NOT EXISTS pants_garment_id CHAR(36) NULL AFTER shirt_garment_id,
    ADD COLUMN IF NOT EXISTS shoes_garment_id CHAR(36) NULL AFTER pants_garment_id,
    ADD COLUMN IF NOT EXISTS score INT NOT NULL DEFAULT 0 AFTER shoes_garment_id,
    ADD COLUMN IF NOT EXISTS rank INT NOT NULL DEFAULT 1 AFTER score,
    ADD COLUMN IF NOT EXISTS requested_garment_ids VARCHAR(255) NULL AFTER label_source;

-- User matching state (lockout + min per type)
CREATE TABLE IF NOT EXISTS eitan_project12.user_matching_state (
    user_id CHAR(36) NOT NULL PRIMARY KEY,
    min_per_type INT NOT NULL DEFAULT 5,
    locked_after_failure TINYINT(1) NOT NULL DEFAULT 0,
    last_failure_at DATETIME NULL,
    last_success_at DATETIME NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_ums_user FOREIGN KEY (user_id) REFERENCES eitan_project12.users(user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Helpful index for garments by owner
CREATE INDEX IF NOT EXISTS idx_garments_owner_type ON eitan_project12.garments(owner_user_id, type);
