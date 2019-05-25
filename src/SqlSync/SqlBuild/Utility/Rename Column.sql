${Title: Rename column  <<Table Name>>.<<Old Column Name>> to <<New Column Name>>}
DECLARE @Table nvarchar(100)
DECLARE @OldColumn nvarchar(100)
DECLARE @NewColumn nvarchar(100)
DECLARE @NewColumnTmp nvarchar(100)
DECLARE @NewColumnTmpQual nvarchar(200)
SET @Table = '<<Table Name>>'
SET @Schema = '<<Table Schema>>'
SET @OldColumn = '<<Old Column Name>>'
SET @NewColumn = '<<New Column Name>>'
SET @OldColumn = @Schema +'.'+@Table+'.'+@OldColumn 
SET @NewColumnTmp = @NewColumn+'_temp'
SET @NewColumnTmpQual = @Schema +'.'+@Table+'.'+@NewColumnTmp
EXECUTE sp_rename @OldColumn, @NewColumnTmp, 'COLUMN'
EXECUTE sp_rename @NewColumnTmpQual, @NewColumn, 'COLUMN'