/**********************************************************
* Database Size Analysis
***********************************************************/

-- Set Database to analyze (set to the current database context)
DECLARE @TableToAnalyze varchar(250)
SET @TableToAnalyze = db_name()
PRINT @TableToAnalyze

-- Create the first temp table for raw data
create table #tmp (
	[schema] varchar(100),
	[name] varchar(100), 
	[rows] int, 
	[reserved] varchar(50), 
	[data] varchar(50), 
	[index_size] varchar(50), 
	[unused] varchar(50))

-- Declare variable for the table name in the cursor 
DECLARE @TableName varchar(100), @NameNoSchema varchar(100)

-- Declate the cursor to loop through tables in the database
DECLARE tableCurs CURSOR FOR 
 	select TABLE_SCHEMA+'.'+Table_Name, Table_Name
	from INFORMATION_SCHEMA.TABLES 
	where table_catalog = @TableToAnalyze and  
	Table_Type = 'BASE TABLE' 

OPEN tableCurs
FETCH NEXT FROM tableCurs INTO @TableName,@NameNoSchema

WHILE @@FETCH_STATUS = 0
BEGIN

	-- Insert raw data in to the temp table
  	insert #tmp ([name], [rows], [reserved],[data],	[index_size],[unused]) exec sp_spaceused @TableName
	UPDATE #tmp SET [name] = @TableName where [name] = @NameNoSchema
  	IF @@ERROR <> 0
	BEGIN
		PRINT 'Table not found '+@TableName
	END
   FETCH NEXT FROM tableCurs INTO @TableName,@NameNoSchema

END

-- Create table variable for numeric and sum values
DECLARE @tblNumeric table(
	[Table Name] varchar(100), 
	[Row Count] int, 
	[Data Size] int, 
	[Index Size] int, 
	[Unused Size] int,
	[Total Reserved Size] int)
	

-- Insert numeric values into table variable
INSERT INTO @tblNumeric 
SELECT 	[name], 
	[rows],
	CAST(REPLACE(data,'KB','') as int),
	CAST(REPLACE(index_size,'KB','') as int),
	CAST(REPLACE(unused,'KB','') as int),
	CAST(REPLACE(reserved,'KB','') as int)

FROM #tmp

-- Select the numeric size data
SELECT distinct * FROM @tblNumeric ORDER BY [Total Reserved Size] desc


-- Clean up
DROP TABLE #tmp
CLOSE tableCurs
DEALLOCATE tableCurs


 



