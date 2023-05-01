using ChatView_API.Models.Db_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatView_API.DAL
{
    public class ChatViewDbContext : DbContext
    {
        public ChatViewDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            new InitializeDB(builder).Seed();
        }

        public DbSet<DbVideo> Videos { get; set; }
        public DbSet<DbContact> ContactForms { get; set; }
    }
}
