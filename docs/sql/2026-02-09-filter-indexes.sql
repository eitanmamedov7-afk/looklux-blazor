-- מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
-- למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
-- לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
-- איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

CREATE INDEX IF NOT EXISTS idx_garments_owner_type ON eitan_project12.garments(owner_user_id, type);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_color ON eitan_project12.garments(owner_user_id, color);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_color2 ON eitan_project12.garments(owner_user_id, color_secondary);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_style_category ON eitan_project12.garments(owner_user_id, style_category);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_season ON eitan_project12.garments(owner_user_id, season);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_material ON eitan_project12.garments(owner_user_id, material);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_brand ON eitan_project12.garments(owner_user_id, brand);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_fit ON eitan_project12.garments(owner_user_id, fit);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_pattern ON eitan_project12.garments(owner_user_id, pattern);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_created ON eitan_project12.garments(owner_user_id, created_at);

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_user_score ON eitan_project12.outfits(user_id, score);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_user_seed_type ON eitan_project12.outfits(user_id, seed_type);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_user_style_label ON eitan_project12.outfits(user_id, style_label);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfits_user_created ON eitan_project12.outfits(user_id, created_at);

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit_type ON eitan_project12.outfit_garments(outfit_id, garment_type);
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_outfit_garments_outfit_seed ON eitan_project12.outfit_garments(outfit_id, is_seed);
