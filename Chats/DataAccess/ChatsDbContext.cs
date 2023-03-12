using Chats.Models;
using Microsoft.EntityFrameworkCore;

namespace Chats.DataAccess;

public class ChatsDbContext : DbContext
{
    public ChatsDbContext()
    {
    }

    public ChatsDbContext(DbContextOptions<ChatsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.Property(e => e.AuthorId).HasMaxLength(450);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Price).HasColumnType("decimal(19, 2)");
            entity.Property(e => e.Title).HasMaxLength(64);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasIndex(e => new { e.AnnouncementId, e.AuthorId }, "IX_Chats_AnnouncementId_AuthorId").IsUnique();
            entity.HasOne(d => d.Announcement).WithMany(p => p.Chats)
                .HasForeignKey(d => d.AnnouncementId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => e.ChatId, "IX_Messages_ChatId");
            entity.Property(e => e.AuthorId).HasMaxLength(450);
            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.HasOne(d => d.Chat).WithMany(p => p.Messages).HasForeignKey(d => d.ChatId);
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MessagesAttachment");
            entity.HasIndex(e => e.MessageId, "IX_MessagesAttachment_MessageId");
            entity.HasIndex(e => e.Name, "IX_MessagesAttachment_Name").IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasOne(d => d.Message).WithMany(p => p.MessageAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessagesAttachment_Messages_MessageId");
        });
    }
}