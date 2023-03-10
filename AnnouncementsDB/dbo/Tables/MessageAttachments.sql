CREATE TABLE [dbo].[MessageAttachments](
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [Name]      NVARCHAR (100)   NOT NULL,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_MessagesAttachment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MessagesAttachment_Messages_MessageId] FOREIGN KEY ([MessageId]) REFERENCES [dbo].[Messages] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_MessagesAttachment_MessageId]
    ON [dbo].[MessageAttachments]([MessageId] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MessagesAttachment_Name]
    ON [dbo].[MessageAttachments]([Name] ASC);
