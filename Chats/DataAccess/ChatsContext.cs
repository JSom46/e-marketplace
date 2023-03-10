using Chats.Models;
using Microsoft.EntityFrameworkCore;

namespace Chats.DataAccess
{
    public class ChatsContext : DbContext
    {
        public ChatsContext(DbContextOptions options) : base(options) { }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
    }
}