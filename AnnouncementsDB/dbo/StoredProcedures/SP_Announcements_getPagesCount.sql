CREATE PROCEDURE [dbo].[SP_Announcements_getPagesCount]
    @PageSize INT = NULL,
    @Title NVARCHAR(64),
    @Category INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageSize IS NULL SET @PageSize = 30;

    DECLARE @TotalRows INT, @TotalPages INT;

    SELECT 
        @TotalRows = COUNT(*) 
    FROM [dbo].[Announcements] 
    WHERE 
        [IsActive] = 1 AND 
        (@Title IS NULL OR [Title] LIKE '%' + @Title + '%') AND 
        (@Category IS NULL OR Category = @Category);

    SELECT CEILING(CAST(@TotalRows AS FLOAT) / @PageSize) AS PagesCount;
END