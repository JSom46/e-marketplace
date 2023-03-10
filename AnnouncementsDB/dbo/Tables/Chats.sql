CREATE TABLE [dbo].[Chats] (
    [Id]             UNIQUEIDENTIFIER NOT NULL,
    [AnnouncementId] UNIQUEIDENTIFIER NOT NULL,
    [AuthorId]       NVARCHAR (450)   NOT NULL,
    CONSTRAINT [PK_Chats] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Chats_Announcements_AnnouncementId] FOREIGN KEY ([AnnouncementId]) REFERENCES [dbo].[Announcements] ([Id]) ON DELETE NO ACTION
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Chats_AnnouncementId_AuthorId]
    ON [dbo].[Chats]([AnnouncementId] ASC, [AuthorId] ASC);
