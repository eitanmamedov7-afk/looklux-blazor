-- SEARCH INDEX
-- OUTFIT_WEAR_LOG, DATABASE, OUTFIT, ADD, HISTORY, TIMESTAMP
--
-- Topic: Outfit wear log SQL table
-- Purpose: Adds an outfit-only history table for "Mark worn" actions.
-- Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT ADD HISTORY TIMESTAMP
-- When to use it: Run this once on eitan_project12 to support outfit worn timestamps.
-- Important notes: This table stores outfit usage history only. It does not duplicate garment data.

CREATE TABLE IF NOT EXISTS eitan_project12.outfit_wear_logs (
    wear_log_id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    outfit_id CHAR(36) NOT NULL,
    worn_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (wear_log_id),
    KEY idx_outfit_wear_logs_user_worn (user_id, worn_at),
    KEY idx_outfit_wear_logs_outfit_worn (outfit_id, worn_at),
    CONSTRAINT fk_outfit_wear_logs_user
        FOREIGN KEY (user_id)
        REFERENCES eitan_project12.users(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_outfit_wear_logs_outfit
        FOREIGN KEY (outfit_id)
        REFERENCES eitan_project12.outfits(outfit_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Upgrade an existing installation created with the earlier duplicate
-- created_at column. worn_at already contains the event timestamp, so
-- dropping created_at preserves every wear record and all summary logic.
SET @drop_created_at = (
    SELECT IF(
        EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = 'eitan_project12'
              AND table_name = 'outfit_wear_logs'
              AND column_name = 'created_at'
        ),
        'ALTER TABLE eitan_project12.outfit_wear_logs DROP COLUMN created_at',
        'SELECT 1'
    )
);
PREPARE drop_created_at_statement FROM @drop_created_at;
EXECUTE drop_created_at_statement;
DEALLOCATE PREPARE drop_created_at_statement;
