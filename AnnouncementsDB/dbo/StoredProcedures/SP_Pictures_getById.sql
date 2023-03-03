CREATE PROCEDURE [dbo].[SP_Pictures_getId]
	@Id BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		[Id], 
		[Name],
		[AnnouncementId]
	FROM [dbo].[Pictures]
	WHERE [Id] = @Id;
END