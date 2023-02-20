using Microsoft.EntityFrameworkCore;
using ShireBank.Repository.Data;
using ShireBank.Repository.Models;
using ShireBank.Repository.Repositories.Interfaces;

namespace ShireBank.Repository.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly DataContext _context;

    public BankAccountRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task<BankAccount> OpenAccount(string firstName, string lastName, decimal debtLimit)
    {
        var isAccountOpened = await _context.Accounts
            .Where(x =>
                x.FirstName == firstName 
                && x.LastName == lastName 
                && !x.IsClosed)
            .AnyAsync();
        
        if (isAccountOpened) 
            return null!;

        var account = new BankAccount
        {
            FirstName = firstName,
            LastName = lastName,
            DebtLimit = debtLimit
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<bool> CloseAccount(uint accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account is not { Balance: 0 }) 
            return false;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> Withdraw(uint accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);

        var available = account.Balance + account.DebtLimit;
        if (available <= 0) 
            return 0;

        var amountToWithdraw = amount > available ? available : amount;

        var newAccountBalance = account.Balance - amountToWithdraw;
        account.Balance = newAccountBalance;

        await _context.SaveChangesAsync();

        return amountToWithdraw;
    }

    public async Task Deposit(uint accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);

        account.Balance += amount;

        await _context.SaveChangesAsync();
    }
}