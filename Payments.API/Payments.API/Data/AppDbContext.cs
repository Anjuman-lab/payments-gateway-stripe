using Microsoft.EntityFrameworkCore;
using Payments.API.Domain.Entities;

namespace Payments.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<TransactionEvent> TransactionEvents => Set<TransactionEvent>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Payment>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.OrderId).IsRequired().HasMaxLength(64);
                e.Property(p => p.Currency).IsRequired().HasMaxLength(3);
                e.Property(p => p.Amount).IsRequired();
                e.HasMany(p => p.Events).WithOne(ev => ev.Payment).HasForeignKey(ev => ev.PaymentId);
            });

            model.Entity<TransactionEvent>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Type).IsRequired().HasMaxLength(24);
                e.Property(t => t.Status).IsRequired().HasMaxLength(24);
            });
        }
    }
}
