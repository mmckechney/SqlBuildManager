DECLARE @SERVER sql_variant 
SELECT @SERVER  = SERVERPROPERTY ('ServerName')
SELECT @SERVER, 'client', name FROM sys.Databases WHERE name like 'Sql%'