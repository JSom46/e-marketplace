CREATE PROCEDURE [dbo].[SP_Pictures_getByAnnouncementId]
	@AnnouncementId UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id], 
		[Name],
		[AnnouncementId]
	FROM [dbo].[Pictures]
	WHERE [AnnouncementId] = @AnnouncementId;
END
