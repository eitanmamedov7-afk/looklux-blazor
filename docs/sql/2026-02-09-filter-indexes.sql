-- 2026-02-09 indexes for dynamic closet/outfit filtering

CREATE INDEX IF NOT EXISTS idx_garments_owner_type ON eitan_project12.garments(owner_user_id, type);
CREATE INDEX IF NOT EXISTS idx_garments_owner_color ON eitan_project12.garments(owner_user_id, color);
CREATE INDEX IF NOT EXISTS idx_garments_owner_color2 ON eitan_project12.garments(owner_user_id, color_secondary);
CREATE INDEX IF NOT EXISTS idx_garments_owner_style_category ON eitan_project12.garments(owner_user_id, style_category);
CREATE INDEX IF NOT EXISTS idx_garments_owner_season ON eitan_project12.garments(owner_user_id, season);
CREATE INDEX IF NOT EXISTS idx_garments_owner_material ON eitan_project12.garments(owner_user_id, material);
CREATE INDEX IF NOT EXISTS idx_garments_owner_brand ON eitan_project12.garments(owner_user_id, brand);
CREATE INDEX IF NOT EXISTS idx_garments_owner_fit ON eitan_project12.garments(owner_user_id, fit);
CREATE INDEX IF NOT EXISTS idx_garments_owner_pattern ON eitan_project12.garments(owner_user_id, pattern);
CREATE INDEX IF NOT EXISTS idx_garments_owner_created ON eitan_project12.garments(owner_user_id, created_at);

CREATE INDEX IF NOT EXISTS idx_outfits_user_score ON eitan_project12.outfits(user_id, score);
CREATE INDEX IF NOT EXISTS idx_outfits_user_seed_type ON eitan_project12.outfits(user_id, seed_type);
CREATE INDEX IF NOT EXISTS idx_outfits_user_style_label ON eitan_project12.outfits(user_id, style_label);
CREATE INDEX IF NOT EXISTS idx_outfits_user_created ON eitan_project12.outfits(user_id, created_at);

CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit_type ON eitan_project12.outfit_garments(outfit_id, garment_type);
CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit_seed ON eitan_project12.outfit_garments(outfit_id, is_seed);
