${Title: Add <<Statistic Name>>}
IF NOT EXISTS (SELECT 1 FROM sys.stats WHERE name = '<<Statistic Name>>')
BEGIN
	<<INSERT>>
END
GO