INSERT INTO sqlbuild_logging (buildfilename, scriptfilename, scriptid, scriptfilehash,
    commitdate, sequence, userid, allowscriptblock, scripttext, tag,
    targetdatabase, runwithversion, buildprojecthash, buildrequestedby,
    scriptrunstart, scriptrunend, description)
VALUES (@BuildFileName, @ScriptFileName, @ScriptId, @ScriptFileHash,
    @CommitDate, @Sequence, @UserId, true, @ScriptText, @Tag,
    @TargetDatabase, @RunWithVersion, @BuildProjectHash, @BuildRequestedBy,
    @ScriptRunStart, @ScriptRunEnd, @Description)
