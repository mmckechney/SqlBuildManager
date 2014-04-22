--types: Table = U ; foreign key = F ; primary key = K ; trigger = TR ;  function = FN,TF ; default = D
IF EXISTS (SELECT 1 FROM dbo.sysobjects WITH (NOLOCK) WHERE name = '<<Constraint Name>>' AND type = 'D')
BEGIN
	ALTER TABLE  [<<Table Schema>>].[<<Table Name>>] DROP CONSTRAINT <<Constraint Name>>
END
GO