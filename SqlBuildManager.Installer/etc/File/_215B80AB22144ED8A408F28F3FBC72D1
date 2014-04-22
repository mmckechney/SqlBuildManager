--***********************************************************************************
GO
-- Sql Build Manager Utility Query: EnforceForeignKeys.sql
-- Runs through all of the tables, and make sure all of the foreign keys are enforced
DECLARE @name varchar(200)
DECLARE namecursor CURSOR FOR SELECT [name]	FROM sys.objects	WHERE (xtype = 'U') 
OPEN namecursor
FETCH NEXT FROM namecursor INTO @name 
WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'Enforcing FK Constraint '+@name 
	EXEC ('ALTER TABLE ' + @name + ' CHECK CONSTRAINT ALL')
    FETCH NEXT FROM namecursor  INTO @name 
END
CLOSE namecursor
DEALLOCATE namecursor
 --************************************************************************************