CREATE PROCEDURE [dbo].[SP_Announcements_delete]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Announcements]
	WHERE [Id] = @Id;

	SELECT @@ROWCOUNT AS RowsAffected;
END