using Microsoft.EntityFrameworkCore;
using Manage.Data.Reminder.Models;

namespace Manage.Data.Reminder
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<Models.Reminder> Reminder { get; set; }
        public DbSet<UserSubscription> UserSubscription { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
