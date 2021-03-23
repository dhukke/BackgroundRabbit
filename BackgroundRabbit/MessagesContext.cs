using Microsoft.EntityFrameworkCore;

namespace BackgroundRabbit
{
    public class MessagesContext: DbContext
    {

        public MessagesContext(DbContextOptions<MessagesContext> options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder  modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}
