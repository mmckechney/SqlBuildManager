INSERT INTO SqlBuild_Logging([BuildFileName],[ScriptFileName],[ScriptId],[ScriptFileHash],[CommitDate],[Sequence],[UserId],[AllowScriptBlock],[ScriptText],[Tag],[TargetDatabase],[RunWithVersion],[BuildProjectHash],[BuildRequestedBy],[ScriptRunStart],[ScriptRunEnd],[Description])
VALUES(@BuildFileName,@ScriptFileName,@ScriptId,@ScriptFileHash,@CommitDate,@Sequence, @UserId, 1,@ScriptText,@Tag,@TargetDatabase,@RunWithVersion,@BuildProjectHash,@BuildRequestedBy, @ScriptRunStart, @ScriptRunEnd, @Description)