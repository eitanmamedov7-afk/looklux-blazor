-- מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
-- למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
-- לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
-- איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

ALTER TABLE eitan_project12.garments
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS feature_json LONGTEXT NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS updated_at DATETIME NULL;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
ALTER TABLE eitan_project12.outfits
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS user_id CHAR(36) NULL AFTER outfit_id,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS shirt_garment_id CHAR(36) NULL AFTER user_id,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS pants_garment_id CHAR(36) NULL AFTER shirt_garment_id,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS shoes_garment_id CHAR(36) NULL AFTER pants_garment_id,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS score INT NOT NULL DEFAULT 0 AFTER shoes_garment_id,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS rank INT NOT NULL DEFAULT 1 AFTER score,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    ADD COLUMN IF NOT EXISTS requested_garment_ids VARCHAR(255) NULL AFTER label_source;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE TABLE IF NOT EXISTS eitan_project12.user_matching_state (
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    user_id CHAR(36) NOT NULL PRIMARY KEY,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    min_per_type INT NOT NULL DEFAULT 5,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    locked_after_failure TINYINT(1) NOT NULL DEFAULT 0,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    last_failure_at DATETIME NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    last_success_at DATETIME NULL,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
    CONSTRAINT fk_ums_user FOREIGN KEY (user_id) REFERENCES eitan_project12.users(user_id)
-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- פקודת מסד נתונים שמכינה או מעדכנת את הסכמה עבור שכבת הדיבי.
CREATE INDEX IF NOT EXISTS idx_garments_owner_type ON eitan_project12.garments(owner_user_id, type);
