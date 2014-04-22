IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'MyTestProcedure' AND ROUTINE_SCHEMA = 'dbo' AND ROUTINE_TYPE = 'PROCEDURE')
BEGIN
	DROP PROCEDURE [dbo].[MyTestProcedure]
END
GO

CREATE PROCEDURE [dbo].[MyTestProcedure]
    @StartProductID [int],
    @CheckDate [datetime]
AS
BEGIN
	/******************************************************************************
	**		Name: [MyTestProcedure]
	**		Desc: Sample Procedure for Unit test
	**              
	**		Auth: me
	**		Date: 1/12/2009
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:		Description:
	**		------		--------	--------------------------------
	**		3/12/2009   Me			Testing
	**		<<date>>   Me			Testing
	*******************************************************************************/

SELECT test, test2 FROM 
	dbo.TestTable WITH (NOLOCK)
	WHERE StartProductID = @StartProductID 
			AND CheckDate = @CheckDate

END;
GO

GRANT EXECUTE ON [dbo].[MyTestProcedure] TO [My_role]