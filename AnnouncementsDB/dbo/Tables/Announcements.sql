CREATE TABLE [dbo].[Announcements]
(
	[Id] UNIQUEIDENTIFIER NOT NULL, 
    [AuthorId] NVARCHAR(450) NOT NULL, 
    [Title] NVARCHAR(64) NOT NULL, 
    [Description] NVARCHAR(4000) NOT NULL, 
    [Category] INT NOT NULL, 
    [CreatedDate] DATETIMEOFFSET NOT NULL, 
    [ExpiresDate] DATETIMEOFFSET NOT NULL, 
    [IsActive] BIT NOT NULL, 
    [Price] DECIMAL(19, 2) NOT NULL,
    CONSTRAINT [PK_Announcements] PRIMARY KEY CLUSTERED ([Id] ASC)
)
