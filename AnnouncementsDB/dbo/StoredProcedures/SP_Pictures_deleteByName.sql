CREATE PROCEDURE [dbo].[SP_Pictures_deleteByName]
	@Name VARCHAR(256)
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Pictures]
	WHERE [Name] = @Name;

	SELECT @@ROWCOUNT AS RowsAffected;
END
