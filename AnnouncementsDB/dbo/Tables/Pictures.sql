CREATE TABLE [dbo].[Pictures]
(
	[Id] BIGINT NOT NULL IDENTITY(1, 1),
	[Name] VARCHAR(256) NOT NULL UNIQUE,
	[AnnouncementId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [FK_Pictures_Announcements] FOREIGN KEY ([AnnouncementId]) REFERENCES [Announcements]([Id])
)
