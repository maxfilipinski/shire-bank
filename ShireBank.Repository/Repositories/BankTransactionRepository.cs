using Microsoft.EntityFrameworkCore;
using ShireBank.Repository.Data;
using ShireBank.Repository.Enums;
using ShireBank.Repository.Models;
using ShireBank.Repository.Repositories.Interfaces;

namespace ShireBank.Repository.Repositories;

public class BankTransactionRepository : IBankTransactionRepository
{
    private readonly DataContext _context;

    public BankTransactionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<BankTransaction> Create(uint accountId, decimal amount, BankTransactionType type)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        var bankTransaction = new BankTransaction(account, amount, type);

        _context.Transactions.Add(bankTransaction);
        await _context.SaveChangesAsync();

        return bankTransaction;
    }

    public async Task<IEnumerable<BankTransaction>> GetAll(uint accountId)
    {
        return await _context.Transactions
            .Where(x => x.Account.Id == accountId)
            .ToListAsync();
    }
}