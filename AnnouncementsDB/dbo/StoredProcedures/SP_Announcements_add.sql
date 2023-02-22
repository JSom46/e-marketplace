CREATE PROCEDURE [dbo].[SP_Announcements_add]
	@AuthorId NVARCHAR(450),
	@Title NVARCHAR(64),
	@Description NVARCHAR(4000),
	@Category INT,
	@CreatedDate DATETIMEOFFSET,
	@ExpiresDate DATETIMEOFFSET,
	@IsActive BIT,
	@Price DECIMAL(10,2)
AS
BEGIN
	SET NOCOUNT ON;

    IF (@Price < 0)
    BEGIN
        RAISERROR('Price must be greater than or equal to zero.', 16, 1);
        RETURN;
    END

    DECLARE @Id UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [dbo].[Announcements] 
	(
		[Id], 
		[AuthorId], 
		[Title], 
		[Description], 
		[Category], 
		[CreatedDate], 
		[ExpiresDate], 
		[IsActive], 
		[Price]
	)
    VALUES 
	(
		@Id, 
		@AuthorId, 
		@Title, 
		@Description, 
		@Category, 
		@CreatedDate, 
		@ExpiresDate, 
		1, 
		@Price
	);

    SELECT @Id AS 'Id';
END