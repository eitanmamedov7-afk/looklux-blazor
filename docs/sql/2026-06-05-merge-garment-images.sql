-- Merge garment_images into garments.
-- Run this after backing up eitan_project12.
-- The app now stores each garment image directly on the garments row.

DROP PROCEDURE IF EXISTS eitan_project12.ensure_garment_merge_columns;

DELIMITER $$

CREATE PROCEDURE eitan_project12.ensure_garment_merge_columns()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'user_id'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD COLUMN user_id CHAR(36) NULL AFTER garment_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'image_bytes'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD COLUMN image_bytes LONGBLOB NULL;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'image_mime_type'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD COLUMN image_mime_type VARCHAR(100) NOT NULL DEFAULT 'image/jpeg';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'sha256'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD COLUMN sha256 VARCHAR(64) NULL;
    END IF;
END$$

DELIMITER ;

CALL eitan_project12.ensure_garment_merge_columns();
DROP PROCEDURE IF EXISTS eitan_project12.ensure_garment_merge_columns;

DROP PROCEDURE IF EXISTS eitan_project12.merge_garment_image_data;

DELIMITER $$

CREATE PROCEDURE eitan_project12.merge_garment_image_data()
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'owner_user_id'
    ) THEN
        UPDATE eitan_project12.garments
        SET user_id = owner_user_id
        WHERE user_id IS NULL;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'garment_hash'
    ) THEN
        UPDATE eitan_project12.garments
        SET sha256 = garment_hash
        WHERE sha256 IS NULL OR TRIM(sha256) = '';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garment_images'
    ) THEN
        UPDATE eitan_project12.garments g
        INNER JOIN eitan_project12.garment_images gi ON gi.garment_id = g.garment_id
        SET g.image_bytes = COALESCE(g.image_bytes, gi.image_bytes),
            g.image_mime_type = COALESCE(NULLIF(gi.mime_type, ''), g.image_mime_type, 'image/jpeg'),
            g.sha256 = COALESCE(NULLIF(g.sha256, ''), gi.sha256);
    END IF;
END$$

DELIMITER ;

CALL eitan_project12.merge_garment_image_data();
DROP PROCEDURE IF EXISTS eitan_project12.merge_garment_image_data;

DROP PROCEDURE IF EXISTS eitan_project12.assert_garment_merge_ready;

DELIMITER $$

CREATE PROCEDURE eitan_project12.assert_garment_merge_ready()
BEGIN
    IF EXISTS (
        SELECT 1
        FROM eitan_project12.garments
        WHERE user_id IS NULL
           OR image_bytes IS NULL
           OR sha256 IS NULL
           OR TRIM(sha256) = ''
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Garment image merge stopped: at least one garment is missing user_id, image_bytes, or sha256.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT user_id, sha256, COUNT(*) AS cnt
            FROM eitan_project12.garments
            GROUP BY user_id, sha256
            HAVING COUNT(*) > 1
        ) duplicates
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Garment image merge stopped: duplicate user_id + sha256 rows exist.';
    END IF;
END$$

DELIMITER ;

CALL eitan_project12.assert_garment_merge_ready();
DROP PROCEDURE IF EXISTS eitan_project12.assert_garment_merge_ready;

DROP PROCEDURE IF EXISTS eitan_project12.drop_garment_merge_indexes;

DELIMITER $$

CREATE PROCEDURE eitan_project12.drop_garment_merge_indexes()
BEGIN
    DECLARE done INT DEFAULT 0;
    DECLARE index_name_value VARCHAR(128);
    DECLARE cur CURSOR FOR
        SELECT DISTINCT index_name
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name IN ('owner_user_id', 'garment_hash', 'image_url', 'updated_at')
          AND index_name <> 'PRIMARY';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    OPEN cur;

    read_loop: LOOP
        FETCH cur INTO index_name_value;
        IF done = 1 THEN
            LEAVE read_loop;
        END IF;

        SET @drop_index_sql = CONCAT('ALTER TABLE eitan_project12.garments DROP INDEX `', index_name_value, '`');
        PREPARE stmt FROM @drop_index_sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;

    CLOSE cur;
END$$

DELIMITER ;

-- Drop foreign keys that use owner_user_id before dropping the old owner_user_id column.
SET @owner_fk_name := (
    SELECT constraint_name
    FROM information_schema.key_column_usage
    WHERE table_schema = 'eitan_project12'
      AND table_name = 'garments'
      AND column_name = 'owner_user_id'
      AND referenced_table_name IS NOT NULL
    LIMIT 1
);

SET @drop_owner_fk_sql := IF(
    @owner_fk_name IS NULL,
    'SELECT 1',
    CONCAT('ALTER TABLE eitan_project12.garments DROP FOREIGN KEY `', @owner_fk_name, '`')
);
PREPARE stmt FROM @drop_owner_fk_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CALL eitan_project12.drop_garment_merge_indexes();
DROP PROCEDURE IF EXISTS eitan_project12.drop_garment_merge_indexes;

ALTER TABLE eitan_project12.garments
    MODIFY user_id CHAR(36) NOT NULL,
    MODIFY image_bytes LONGBLOB NOT NULL,
    MODIFY image_mime_type VARCHAR(100) NOT NULL DEFAULT 'image/jpeg',
    MODIFY sha256 VARCHAR(64) NOT NULL;

DROP PROCEDURE IF EXISTS eitan_project12.drop_garment_merge_columns;

DELIMITER $$

CREATE PROCEDURE eitan_project12.drop_garment_merge_columns()
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'owner_user_id'
    ) THEN
        ALTER TABLE eitan_project12.garments DROP COLUMN owner_user_id;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'image_url'
    ) THEN
        ALTER TABLE eitan_project12.garments DROP COLUMN image_url;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'garment_hash'
    ) THEN
        ALTER TABLE eitan_project12.garments DROP COLUMN garment_hash;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND column_name = 'updated_at'
    ) THEN
        ALTER TABLE eitan_project12.garments DROP COLUMN updated_at;
    END IF;
END$$

DELIMITER ;

CALL eitan_project12.drop_garment_merge_columns();
DROP PROCEDURE IF EXISTS eitan_project12.drop_garment_merge_columns;

DROP PROCEDURE IF EXISTS eitan_project12.ensure_garment_merge_keys;

DELIMITER $$

CREATE PROCEDURE eitan_project12.ensure_garment_merge_keys()
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND constraint_type = 'FOREIGN KEY'
          AND constraint_name = 'fk_garments_user'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD CONSTRAINT fk_garments_user
            FOREIGN KEY (user_id) REFERENCES eitan_project12.users(user_id)
            ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'uq_garments_user_sha256'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD UNIQUE KEY uq_garments_user_sha256 (user_id, sha256);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_type'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_type (user_id, type);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_created'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_created (user_id, created_at);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_color'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_color (user_id, color);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_color2'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_color2 (user_id, color_secondary);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_style_category'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_style_category (user_id, style_category);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_season'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_season (user_id, season);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_material'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_material (user_id, material);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_brand'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_brand (user_id, brand);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_fit'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_fit (user_id, fit);
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.statistics
        WHERE table_schema = 'eitan_project12'
          AND table_name = 'garments'
          AND index_name = 'idx_garments_user_pattern'
    ) THEN
        ALTER TABLE eitan_project12.garments
            ADD INDEX idx_garments_user_pattern (user_id, pattern);
    END IF;
END$$

DELIMITER ;

CALL eitan_project12.ensure_garment_merge_keys();
DROP PROCEDURE IF EXISTS eitan_project12.ensure_garment_merge_keys;

DROP TABLE IF EXISTS eitan_project12.garment_images;
