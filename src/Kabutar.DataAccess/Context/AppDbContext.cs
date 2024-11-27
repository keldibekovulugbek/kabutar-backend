using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;


namespace Kabutar.DataAccess.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; } = null!;

    public virtual DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filters
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Message>()
            .HasQueryFilter(m => !m.IsDeletedBySender && !m.IsDeletedByReceiver);
    }

}
