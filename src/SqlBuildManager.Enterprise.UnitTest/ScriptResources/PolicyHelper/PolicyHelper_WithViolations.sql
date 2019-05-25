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
	**		3/12/2008   Me			Testing
	**		4/1/2008    Me			Testing
	*******************************************************************************/

SELECT * FROM 
	TestTable 
	WHERE StartProductID = @StartProductID 
			AND CheckDate = @CheckDate

END;
GO

GRANT EXECUTE ON [dbo].[MyTestProcedure] TO [public]
