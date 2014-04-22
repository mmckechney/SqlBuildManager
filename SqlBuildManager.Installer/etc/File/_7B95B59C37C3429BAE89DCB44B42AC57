
/**********************************************************************
	Runs through all of the Stored Procs and functions an ensures that
	the listed user id's have the appropriate execute,select and insert
	permissions
**********************************************************************/
SET NOCOUNT ON

DECLARE @name varchar(200)
DECLARE @type varchar(2)
DECLARE @id nvarchar(250)
DECLARE @err int
--Add or remove the desired Id's here
CREATE TABLE #ids([id] nvarchar(250))
INSERT INTO #ids VALUES('public')

--Declare the cursor to loop through the id's
DECLARE idcursor CURSOR FOR 
	SELECT [id]	FROM #ids WITH (NOLOCK)

--Declare the cursor to loop through the procs and functions
DECLARE spcursor CURSOR FOR 
	SELECT [name], [xtype]
	FROM dbo.sysobjects WHERE xtype IN('P','TF','FN','U') AND (category = 0) ORDER BY [xtype], [name]

OPEN spcursor
FETCH NEXT FROM spcursor INTO @name,@type
WHILE @@FETCH_STATUS = 0
BEGIN
	--Print the object name and type
	PRINT @name +'('+@type+')'

	OPEN idcursor
	FETCH NEXT FROM idcursor INTO @id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--Grant permission to the id
		if exists (select 1 from dbo.sysusers where name = @id)
			BEGIN
				IF @type = 'P' OR @type = 'FN'  -- Procs and scalar functions need EXECUTE
					BEGIN 
						IF OBJECT_ID(@name) IS NOT NULL
						BEGIN
							EXEC ('GRANT EXECUTE ON ' + @name + ' TO '+ @id)
							SET @err = @@ERROR
							IF @err <> 0 
								BEGIN 
									PRINT 'Unable to grant EXECUTE on ' + @name +' to '+@id 
								END
							ELSE
								BEGIN
									PRINT @id +' granted EXECUTE to ' + @name
								END
						END
							
					END
				ELSE  
					BEGIN -- tables and table functions need SELECT
						IF OBJECT_ID(@name) IS NOT NULL
						BEGIN
							EXEC ('GRANT SELECT ON ' + @name + ' TO '+ @id)
							SET @err = @@ERROR
							IF @err <> 0 
								BEGIN 
									PRINT 'Unable to grant SELECT on ' + @name +' to ' +@id
								END
							ELSE
								BEGIN
									PRINT @id +' granted SELECT to ' + @name
								END
						
							IF @type = 'U'
								BEGIN
									EXEC ('GRANT INSERT ON ' + @name + ' TO '+ @id)
									SET @err = @@ERROR
									IF @err <> 0 
										BEGIN 
											PRINT 'Unable to grant INSERT on ' + @name +' to ' +@id
										END
									ELSE
										BEGIN
											PRINT @id +' granted INSERT to ' + @name
										END
								END
						END
						
					END
			END
		FETCH NEXT FROM idcursor INTO @id
	END
	close idcursor
	
	FETCH NEXT FROM spcursor  INTO @name,@type
END

--Clean up
CLOSE spcursor
DEALLOCATE spcursor
DEALLOCATE idcursor
DROP TABLE  #ids