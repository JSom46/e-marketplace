CREATE PROCEDURE [dbo].[SP_Pictures_deleteByAnnouncementId]
	@AnnouncementId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Pictures]
	WHERE [AnnouncementId] = @AnnouncementId;

	SELECT @@ROWCOUNT AS RowsAffected;
END
