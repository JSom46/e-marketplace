CREATE PROCEDURE [dbo].[SP_Pictures_delete]
	@Id BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Pictures]
	WHERE [Id] = @Id;

	SELECT @@ROWCOUNT AS RowsAffected;
END
