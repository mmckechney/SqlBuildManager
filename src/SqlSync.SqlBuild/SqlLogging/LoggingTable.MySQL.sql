CREATE TABLE IF NOT EXISTS sqlbuild_logging (
    buildfilename VARCHAR(300) NOT NULL,
    scriptfilename VARCHAR(300) NOT NULL,
    scriptid CHAR(36) NOT NULL,
    scriptfilehash VARCHAR(100) NULL,
    commitdate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    sequence INT NOT NULL DEFAULT 0,
    userid VARCHAR(100) NULL,
    allowscriptblock TINYINT(1) NOT NULL DEFAULT 1,
    allowblockupdateid VARCHAR(200) NULL,
    scripttext LONGTEXT NULL,
    tag VARCHAR(200) NULL,
    targetdatabase VARCHAR(200) NULL,
    runwithversion VARCHAR(50) NULL,
    buildprojecthash VARCHAR(100) NULL,
    buildrequestedby VARCHAR(256) NULL,
    scriptrunstart DATETIME(6) NULL,
    scriptrunend DATETIME(6) NULL,
    description VARCHAR(500) NULL,
    KEY ix_sqlbuild_logging (buildfilename),
    KEY ix_sqlbuild_logging_1 (scriptfilename),
    KEY ix_sqlbuild_logging_2 (scriptid),
    KEY ix_sqlbuild_logging_commitcheck (scriptid, commitdate DESC)
);

ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS scripttext LONGTEXT NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS tag VARCHAR(200) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS targetdatabase VARCHAR(200) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS runwithversion VARCHAR(50) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS buildprojecthash VARCHAR(100) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS buildrequestedby VARCHAR(256) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS scriptrunstart DATETIME(6) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS scriptrunend DATETIME(6) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS description VARCHAR(500) NULL;
ALTER TABLE sqlbuild_logging ADD COLUMN IF NOT EXISTS allowblockupdateid VARCHAR(200) NULL;

ALTER TABLE sqlbuild_logging MODIFY COLUMN buildrequestedby VARCHAR(256) NULL;
ALTER TABLE sqlbuild_logging MODIFY COLUMN description VARCHAR(500) NULL;
