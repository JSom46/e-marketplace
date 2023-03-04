CREATE PROCEDURE [dbo].[SP_Announcements_getByAuthorId]
	@AuthorId NVARCHAR(450)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
        [Id], 
        [AuthorId], 
        [Title], 
        [Category], 
        [CreatedDate], 
        [ExpiresDate], 
        [IsActive],
        [Price],
        (SELECT TOP 1 P.[Name] FROM [dbo].[Pictures] P WHERE P.AnnouncementId = A.Id) AS PictureName
	FROM [dbo].[Announcements] A
	WHERE [AuthorId] = @AuthorId;
END
