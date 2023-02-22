CREATE PROCEDURE [dbo].[SP_Announcements_getPage]
    @PageNumber INT = NULL,
    @PageSize INT = NULL,
    @Title NVARCHAR(64),
    @SortColumn NVARCHAR(64) = NULL,
    @Ascending BIT = NULL,
    @Category INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber IS NULL SET @PageNumber = 1;
    IF @PageSize IS NULL SET @PageSize = 30;
	IF @SortColumn IS NULL SET @SortColumn = 'CreatedDate';
    IF @Ascending IS NULL SET @Ascending = 0;

    DECLARE @TotalRows INT;

    SELECT 
        @TotalRows = COUNT(*) 
    FROM [dbo].[Announcements] 
    WHERE 
        [IsActive] = 1 AND 
        (@Title IS NULL OR [Title] LIKE '%' + @Title + '%') AND 
        (@Category IS NULL OR Category = @Category);

    DECLARE @StartIndex INT = (@PageNumber - 1) * @PageSize + 1;
    DECLARE @EndIndex INT = @StartIndex + @PageSize - 1;

    WITH Announcements AS
    (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY 
                CASE WHEN @SortColumn = 'Price' AND @Ascending = 0 THEN [Price] END DESC,
                CASE WHEN @SortColumn = 'Price' AND @Ascending = 1 THEN [Price] END ASC,
                CASE WHEN @SortColumn = 'CreatedDate' AND @Ascending = 0 THEN [CreatedDate] END DESC)
            AS 'RowNumber', 
            [Id], 
            [AuthorId], 
            [Title], 
            [Description], 
            [Category], 
            [CreatedDate], 
            [ExpiresDate], 
            [Price]
        FROM [dbo].[Announcements]
        WHERE 
            [IsActive] = 1 AND 
            (@Title IS NULL OR [Title] LIKE '%' + @Title + '%') AND 
            [Price] >= 0
    )
    SELECT 
        [Id], 
        [AuthorId], 
        [Title], 
        [Description], 
        [Category], 
        [CreatedDate], 
        [ExpiresDate], 
        [Price]
    FROM Announcements
    WHERE [RowNumber] BETWEEN @StartIndex AND @EndIndex;
END