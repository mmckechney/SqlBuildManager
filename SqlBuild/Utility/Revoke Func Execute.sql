${Title: Revoke Execute of <<Function Name>>}
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = '<<Function Name>>' AND ROUTINE_SCHEMA = '<<Function Schema>>'AND ROUTINE_TYPE = 'FUNCTION')
BEGIN
	REVOKE EXECUTE ON [<<Function Schema>>].[<<Function Name>>] TO [<<User or Role>>]
END
GO