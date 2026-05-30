-- מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
-- למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
-- לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
-- איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

ALTER TABLE eitan_project12.garments
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS color_secondary VARCHAR(64) NULL AFTER color,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS pattern VARCHAR(64) NULL AFTER color_secondary,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS style_category VARCHAR(64) NULL AFTER pattern,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS season VARCHAR(32) NULL AFTER style_category,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS occasion VARCHAR(64) NULL AFTER season,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS formality_level TINYINT NULL AFTER occasion,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS style_tags JSON NULL AFTER formality_level;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
ALTER TABLE eitan_project12.outfits
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS style_label VARCHAR(120) NULL AFTER rank,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS explanation TEXT NULL AFTER style_label,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS recommended_places TEXT NULL AFTER explanation,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS seed_type VARCHAR(32) NULL AFTER recommended_places;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE TABLE IF NOT EXISTS eitan_project12.outfit_garments (
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    outfit_garment_id CHAR(36) NOT NULL PRIMARY KEY,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    outfit_id CHAR(36) NOT NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    garment_id CHAR(36) NOT NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    garment_type VARCHAR(16) NOT NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    is_seed TINYINT(1) NOT NULL DEFAULT 0,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    CONSTRAINT fk_outfit_garments_outfit FOREIGN KEY (outfit_id)
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
        REFERENCES eitan_project12.outfits(outfit_id)
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
        ON DELETE CASCADE,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    CONSTRAINT fk_outfit_garments_garment FOREIGN KEY (garment_id)
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
        REFERENCES eitan_project12.garments(garment_id)
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
        ON DELETE CASCADE
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit ON eitan_project12.outfit_garments(outfit_id);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfit_garments_garment ON eitan_project12.outfit_garments(garment_id);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_score ON eitan_project12.outfits(score);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_seed_type ON eitan_project12.outfits(seed_type);
