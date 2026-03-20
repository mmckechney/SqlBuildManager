CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging ON sqlbuild_logging (buildfilename);
CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging_1 ON sqlbuild_logging (scriptfilename);
CREATE INDEX IF NOT EXISTS ix_sqlbuild_logging_commitcheck ON sqlbuild_logging (scriptid, commitdate DESC);
