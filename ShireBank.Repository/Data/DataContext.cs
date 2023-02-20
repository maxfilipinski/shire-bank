using Microsoft.EntityFrameworkCore;
using ShireBank.Repository.Models;

namespace ShireBank.Repository.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<BankAccount> Accounts => Set<BankAccount>();
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>()
            .ToTable("Accounts");
        
        modelBuilder.Entity<BankAccount>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<BankAccount>()
            .Property(x => x.FirstName)
            .IsRequired();

        modelBuilder.Entity<BankAccount>()
            .Property(x => x.LastName)
            .IsRequired();

        modelBuilder.Entity<BankAccount>()
            .Property(x => x.DebtLimit)
            .IsRequired();

        modelBuilder.Entity<BankAccount>()
            .Property(x => x.Balance)
            .HasDefaultValue(0f)
            .IsRequired();

        modelBuilder.Entity<BankAccount>()
            .Property(x => x.IsClosed)
            .HasDefaultValue(false)
            .IsRequired();

        modelBuilder.Entity<BankTransaction>()
            .ToTable("Transactions");

        modelBuilder.Entity<BankTransaction>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<BankTransaction>()
            .Property(x => x.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow)
            .IsRequired();

        modelBuilder.Entity<BankTransaction>()
            .Property(x => x.Value)
            .IsRequired();

        modelBuilder.Entity<BankTransaction>()
            .Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        modelBuilder.Entity<BankTransaction>()
            .HasOne(x => x.Account)
            .WithMany(x => x.Transactions)
            .IsRequired();
    }
}