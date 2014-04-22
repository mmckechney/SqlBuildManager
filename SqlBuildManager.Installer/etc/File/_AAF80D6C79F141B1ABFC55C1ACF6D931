${Title: Drop <<Function Name>>}
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = '<<Function Name>>' AND ROUTINE_SCHEMA = '<<Function Schema>>' AND ROUTINE_TYPE = 'FUNCTION')
BEGIN
	DROP FUNCTION [<<Function Schema>>].[<<Function Name>>]
END
GO
