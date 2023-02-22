using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ShireBank.Repository.Data;
using ShireBank.Repository.Repositories;
using ShireBank.Repository.Repositories.Interfaces;

namespace ShireBank.Repository.Tests.Repositories;

[TestFixture]
public class BankAccountRepositoryTests
{
    [Test]
    public async Task OpenAccount_Should_Open_Account()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

        await using var context = new DataContext(options);
        await context.Database.EnsureCreatedAsync();
        
        var repository = new BankAccountRepository(context);

        // Act
        var openedAccount = await repository.OpenAccount("Jan", "Kowalski", 999.9m);
        var accountFromDb = await repository.GetAccount(openedAccount.Id);

        // Assert
        Assert.NotNull(accountFromDb);
    }

    [Test]
    public async Task OpenAccount_Should_Not_Open_Account_If_Account_Already_Opened()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

        await using var context = new DataContext(options);
        await context.Database.EnsureCreatedAsync();
        
        var repository = new BankAccountRepository(context);

        // Act
        await repository.OpenAccount("Jan", "Kowalski", 999.9m);
        var secondAccount = await repository.OpenAccount("Jan", "Kowalski", 999.9m);

        // Assert
        Assert.Null(secondAccount);
    }

    [Test]
    public async Task CloseAccount_Should_Close_Account()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

        await using var context = new DataContext(options);
        await context.Database.EnsureCreatedAsync();
        
        var repository = new BankAccountRepository(context);
        
        // Act
        var account = await repository.OpenAccount("Jan", "Kowalski", 0m);
        var isClosed = await repository.CloseAccount(account.Id);

        // Assert
        Assert.True(isClosed);
    }

    [Test]
    public async Task CloseAccount_Should_Not_Close_Account_With_Outstanding_Balance()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(connection).Options;

        await using var context = new DataContext(options);
        await context.Database.EnsureCreatedAsync();
        
        var repository = new BankAccountRepository(context);
        
        // Act
        var account = await repository.OpenAccount("Jan", "Kowalski", 0m);
        account.Balance = 100.0m;
        await context.SaveChangesAsync();

        var isClosed = await repository.CloseAccount(account.Id);

        // Assert
        Assert.False(isClosed);
    }
}