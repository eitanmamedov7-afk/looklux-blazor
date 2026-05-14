-- 2026-02-09 matcher metadata + extended garment features + outfit_garments relation

-- Extended garment feature columns for stronger matching context.
ALTER TABLE eitan_project12.garments
    ADD COLUMN IF NOT EXISTS color_secondary VARCHAR(64) NULL AFTER color,
    ADD COLUMN IF NOT EXISTS pattern VARCHAR(64) NULL AFTER color_secondary,
    ADD COLUMN IF NOT EXISTS style_category VARCHAR(64) NULL AFTER pattern,
    ADD COLUMN IF NOT EXISTS season VARCHAR(32) NULL AFTER style_category,
    ADD COLUMN IF NOT EXISTS occasion VARCHAR(64) NULL AFTER season,
    ADD COLUMN IF NOT EXISTS formality_level TINYINT NULL AFTER occasion,
    ADD COLUMN IF NOT EXISTS style_tags JSON NULL AFTER formality_level;

-- Outfit metadata persisted for each recommendation.
ALTER TABLE eitan_project12.outfits
    ADD COLUMN IF NOT EXISTS style_label VARCHAR(120) NULL AFTER rank,
    ADD COLUMN IF NOT EXISTS explanation TEXT NULL AFTER style_label,
    ADD COLUMN IF NOT EXISTS recommended_places TEXT NULL AFTER explanation,
    ADD COLUMN IF NOT EXISTS seed_type VARCHAR(32) NULL AFTER recommended_places;

-- Relation table: one row per garment used in each saved outfit.
CREATE TABLE IF NOT EXISTS eitan_project12.outfit_garments (
    outfit_garment_id CHAR(36) NOT NULL PRIMARY KEY,
    outfit_id CHAR(36) NOT NULL,
    garment_id CHAR(36) NOT NULL,
    garment_type VARCHAR(16) NOT NULL,
    is_seed TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_outfit_garments_outfit FOREIGN KEY (outfit_id)
        REFERENCES eitan_project12.outfits(outfit_id)
        ON DELETE CASCADE,
    CONSTRAINT fk_outfit_garments_garment FOREIGN KEY (garment_id)
        REFERENCES eitan_project12.garments(garment_id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit ON eitan_project12.outfit_garments(outfit_id);
CREATE INDEX IF NOT EXISTS idx_outfit_garments_garment ON eitan_project12.outfit_garments(garment_id);
CREATE INDEX IF NOT EXISTS idx_outfits_score ON eitan_project12.outfits(score);
CREATE INDEX IF NOT EXISTS idx_outfits_seed_type ON eitan_project12.outfits(seed_type);
