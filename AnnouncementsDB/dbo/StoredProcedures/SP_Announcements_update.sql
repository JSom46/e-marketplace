CREATE PROCEDURE [dbo].[SP_Announcements_update]
	@Id UNIQUEIDENTIFIER,
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

	UPDATE [dbo].[Announcements]
	SET
	[AuthorId] = @AuthorId,
	[Title] = @Title,
	[Description] = @Description,
	[Category] = @Category,
	[CreatedDate] = @CreatedDate,
	[ExpiresDate] = @ExpiresDate,
	[IsActive] = @IsActive,
	[Price] = @Price
	WHERE [Id] = @Id;

	SELECT @@ROWCOUNT AS 'RowsAffected';
END
