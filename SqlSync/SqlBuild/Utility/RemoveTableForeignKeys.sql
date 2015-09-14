--***************************************************************************
GO
-- Sql Build Manager Utility Query: RemoveTableForeignKeys.sql
-- Finds all of the FKs owned by a table and drops them
DECLARE @TableName varchar(256)
SET @TableName = '<<Table Name>>'
SET @Schema = '<<Table Schema>>'
DECLARE @FkName varchar(500)
DECLARE fkCursor CURSOR FOR 
	SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE Table_name = @TableName AND
			  CONSTRAINT_TYPE = 'FOREIGN KEY'

OPEN fkCursor
FETCH NEXT FROM fkCursor INTO @FkName 
WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Droping Constraint '+@FkName +' off Table '+@Schema+'.'+@TableName
		EXEC ('ALTER TABLE '+@Schema+'.'+@TableName+' DROP CONSTRAINT '+@FkName)
		FETCH NEXT FROM fkCursor INTO @FkName 
	END
CLOSE fkCursor
DEALLOCATE fkCursor 
GO
--***************************************************************************