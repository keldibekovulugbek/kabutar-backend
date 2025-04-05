using Kabutar.Domain.Entities.Messages;
using Kabutar.Domain.Entities.Users;
using Kabutar.Domain.Entities.Attachments;
using Kabutar.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Kabutar.DataAccess.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<Message> Messages { get; set; } = null!;
    public virtual DbSet<Attachment> Attachments { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name));

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrWhiteSpace(keyName))
                    key.SetName(ToSnakeCase(keyName));
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                var constraintName = fk.GetConstraintName();
                if (!string.IsNullOrWhiteSpace(constraintName))
                    fk.SetConstraintName(ToSnakeCase(constraintName));
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.Name;
                if (!string.IsNullOrWhiteSpace(indexName))
                    index.SetDatabaseName(ToSnakeCase(indexName));
            }
        }

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ Soft delete filters
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Message>().HasQueryFilter(m => !m.IsDeletedBySender && !m.IsDeletedByReceiver);
        modelBuilder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<Auditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = now;
                entry.Entity.Updated = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.Updated = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(
            input,
            @"([a-z0-9])([A-Z])",
            "$1_$2",
            RegexOptions.Compiled).ToLower();
    }

}
