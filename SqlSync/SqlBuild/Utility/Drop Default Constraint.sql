${Title: Drop Default Constraint on <<Table Schema>>.<<Table Name>>.<<Column Name>>}
DECLARE @constraint nvarchar(100)
DECLARE @sql nvarchar(500)
select @constraint = name from dbo.sysobjects where id in (
	select cdefault FROM dbo.syscolumns where name = '<<Column Name>>' and id in 
	(select id from dbo.sysobjects where name = '<<Table Name>>'))
SET @sql = 'ALTER TABLE <<Table Name>> DROP CONSTRAINT '+ @constraint
IF @constraint IS NOT NULL
BEGIN
	PRINT 'Dropping Constraint ['+@constraint+'] off [<<Table Schema>>].[<<Table Name>>].[<<Column Name>>]'
	EXEC sp_executesql @sql
END
ELSE
BEGIN
 	PRINT 'No Constraint on Table [<<Table Schema>>].[<<Table Name>>].[<<Column Name>>]'
END