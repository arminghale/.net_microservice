using Microsoft.EntityFrameworkCore;
using Manage.Data.Identity.Models;

namespace Manage.Data.Identity
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<User> User { get; set; }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<Domain> Domain { get; set; }
        public DbSet<DomainValue> DomainValue { get; set; }
        public DbSet<SubDomainValue> SubDomainValue { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Models.Action> Action { get; set; }
        public DbSet<ActionGroup> ActionGroup { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UACC> UACC { get; set; }
        public DbSet<RACC> RACC { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubDomainValue>(
                op =>
                {
                    op.HasOne(p => p.Parent).WithMany(d => d.AsParent).HasForeignKey(a => a.ParentId).OnDelete(DeleteBehavior.Cascade);
                    op.HasOne(p => p.Child).WithMany(d => d.AsChild).HasForeignKey(a => a.ChildId);
                });

        }
    }
}
