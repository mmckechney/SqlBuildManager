IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.Columns WHERE TABLE_NAME = '<<Table Name>>' AND TABLE_SCHEMA = '<<Table Schema>>' AND COLUMN_NAME = '<<Column Name>>')
BEGIN
	<<INSERT>>
END
GO