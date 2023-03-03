CREATE PROCEDURE [dbo].[SP_Pictures_add]
	@AnnouncementId UNIQUEIDENTIFIER,
	@Name VARCHAR(256)
AS
BEGIN
	INSERT INTO [dbo].[Pictures] 
	(
		[Name], 
		[AnnouncementId]
	) 
	VALUES 
	(
		@Name, 
		@AnnouncementId
	);
END
