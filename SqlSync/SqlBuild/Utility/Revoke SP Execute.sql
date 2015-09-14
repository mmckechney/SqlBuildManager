${Title: Revoke Execute of <<Stored Proc Name>>}
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WITH (NOLOCK) WHERE ROUTINE_NAME = '<<Stored Proc Name>>' AND ROUTINE_SCHEMA = '<<Stored Proc Schema>>' AND ROUTINE_TYPE = 'PROCEDURE')
BEGIN
	REVOKE EXECUTE ON [<<Stored Proc Schema>>].[<<Stored Proc Name>>] TO [<<User or Role>>]
END
GO