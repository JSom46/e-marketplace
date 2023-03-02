CREATE PROCEDURE [dbo].[SP_Announcements_getById]
	@Id UNIQUEIDENTIFIER
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
	WHERE [Id] = @Id;
END
