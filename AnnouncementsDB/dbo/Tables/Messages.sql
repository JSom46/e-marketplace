CREATE TABLE [dbo].[Messages] (
    [Id]          UNIQUEIDENTIFIER   NOT NULL,
    [AuthorId]    NVARCHAR (450)     NOT NULL,
    [Content]     NVARCHAR (2000)    NOT NULL,
    [CreatedDate] DATETIMEOFFSET (7) NOT NULL,
    [Received]    BIT                NOT NULL,
    [ChatId]      UNIQUEIDENTIFIER   NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Messages_Chats_ChatId] FOREIGN KEY ([ChatId]) REFERENCES [dbo].[Chats] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_Messages_ChatId]
    ON [dbo].[Messages]([ChatId] ASC);
