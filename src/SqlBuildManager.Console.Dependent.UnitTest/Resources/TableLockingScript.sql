BEGIN TRANSACTION
INSERT INTO dbo.TransactionTest VALUES ('PROCESS LOCK', newid(), getdate())
DECLARE @Count INT
SET @Count=0
WHILE @Count < {0}   --near infinite loop...
BEGIN  
       SELECT TOP 1 *  FROM dbo.TransactionTest WITH (TABLOCKX)
       SET @Count = @Count+1
END  
COMMIT TRANSACTION