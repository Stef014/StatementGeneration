using AccountsStatementsData.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountsStatementsData;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Statement> Statements => Set<Statement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.AccountId);
            entity.Property(a => a.AccountId).ValueGeneratedOnAdd();
            entity.Property(a => a.AccountHolderName).IsRequired().HasMaxLength(255);
            entity.Property(a => a.ClosingBalance).IsRequired();
        });
        
        modelBuilder.Entity<Statement>(entity =>
        {
            entity.HasKey(s => s.StatementId);
            entity.Property(s => s.StatementId).ValueGeneratedOnAdd();
            entity.Property(s => s.AccountId).IsRequired();
            entity.Property(s => s.StartTimestamp).IsRequired();
            entity.Property(s => s.EndTimestamp).IsRequired();
            entity.Property(s => s.DownloadUrl).IsRequired().HasMaxLength(500);

            entity.HasOne(s => s.Account)
                  .WithMany(a => a.Statements)
                  .HasForeignKey(s => s.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
