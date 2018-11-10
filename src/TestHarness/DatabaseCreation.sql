CREATE DATABASE DataFrameworkTest
GO

USE DataFrameworkTest
GO

CREATE PROCEDURE TestMultiDataSet
(
	@stringInput NVARCHAR(MAX),
	@guidOutput UNIQUEIDENTIFIER OUTPUT
)
AS
BEGIN
	-- Turn off rowcount
	SET NOCOUNT ON

	-- Select output params
	SELECT @guidOutput = NEWID();

	-- Return first dataset
	SELECT @stringInput + ' Processed'

	-- Return next dataset
	SELECT 'Row 1', 'Second Column (1)'
	UNION
	SELECT 'Row 2', 'Second Column (2)'
	UNION
	SELECT 'Row 3', 'Second Column (3)'
	UNION
	SELECT 'Row 4', 'Second Column (4)'

	-- Return final data set
	SELECT 'Row 1', 1, NEWID(), 1
	UNION
	SELECT 'Row 2', 2, NEWID(), 1
	UNION
	SELECT 'Row 3', 2, NEWID(), 0

	-- Return success
	RETURN 0
END
GO