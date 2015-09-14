${Title: Drop <<Stored Proc Name>>}
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = '<<Stored Proc Name>>' AND ROUTINE_SCHEMA = '<<Stored Proc Schema>>' AND ROUTINE_TYPE = 'PROCEDURE')
BEGIN
	DROP PROCEDURE [<<Stored Proc Schema>>].[<<Stored Proc Name>>]
END
GO
