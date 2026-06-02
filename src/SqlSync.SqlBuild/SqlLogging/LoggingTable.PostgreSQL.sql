CREATE TABLE IF NOT EXISTS sqlbuild_logging (
    buildfilename VARCHAR(300) NOT NULL,
    scriptfilename VARCHAR(300) NOT NULL,
    scriptid UUID NOT NULL,
    scriptfilehash VARCHAR(100) NULL,
    commitdate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sequence INT NOT NULL DEFAULT 0,
    userid VARCHAR(100) NULL,
    allowscriptblock BOOLEAN NOT NULL DEFAULT TRUE,
    allowblockupdateid VARCHAR(200) NULL,
    scripttext TEXT NULL,
    tag VARCHAR(200) NULL,
    targetdatabase VARCHAR(200) NULL,
    runwithversion VARCHAR(50) NULL,
    buildprojecthash VARCHAR(100) NULL,
    buildrequestedby VARCHAR(256) NULL,
    scriptrunstart TIMESTAMP NULL,
    scriptrunend TIMESTAMP NULL,
    description VARCHAR(500) NULL
);

ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS buildrequestedby VARCHAR(256);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'sqlbuild_logging'
          AND column_name = 'buildrequestedby'
          AND character_maximum_length IS NOT NULL
          AND character_maximum_length < 256
    ) THEN
        ALTER TABLE sqlbuild_logging ALTER COLUMN buildrequestedby TYPE VARCHAR(256);
    END IF;
END $$;
