﻿IF NOT EXISTS(SELECT 1 FROM SqlBuild_Logging WHERE [ScriptId] = '{0}') BEGIN INSERT INTO SqlBuild_Logging([BuildFileName],[ScriptFileName],[ScriptId],[ScriptFileHash],[CommitDate],[Sequence],[UserId],[AllowScriptBlock],[ScriptText],[Tag],[TargetDatabase])
VALUES('TestPreRun','TestPreRunScript','{0}','MadeUpHash',getdate(),1, 'TestUser', 1,'Testing','','TestDatabase') END