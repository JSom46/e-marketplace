CREATE PROCEDURE [dbo].[SP_Announcements_getByAuthorId]
	@AuthorId NVARCHAR(450)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
        [Id], 
        [AuthorId], 
        [Title], 
        [Description], 
        [Category], 
        [CreatedDate], 
        [ExpiresDate], 
        [IsActive],
        [Price]
	FROM [dbo].[Announcements]
	WHERE [AuthorId] = @AuthorId;
END
